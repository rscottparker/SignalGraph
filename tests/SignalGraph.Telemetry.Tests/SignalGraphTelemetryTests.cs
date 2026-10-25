using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace SignalGraph.Telemetry.Tests;

public sealed class SignalGraphTelemetryTests : IDisposable
{
    private readonly List<Activity> _exported = [];
    private readonly TracerProvider _provider;
    private readonly SignalGraphTelemetry _signals;

    public SignalGraphTelemetryTests()
    {
        const string serviceName = "signalgraph-tests";

        _provider = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .AddInMemoryExporter(_exported)
            .Build();

        _signals = new SignalGraphTelemetry(new SignalGraphOptions
        {
            ServiceName = serviceName,
            ServiceVersion = "1.0.0-test",
        });
    }

    [Fact]
    public void StartOperation_records_name_timing_and_attributes()
    {
        using (var op = _signals.StartOperation("orders.place", o => o
                   .Attribute("order.id", "ord-1")
                   .Attribute("user.id", 42)))
        {
            Assert.Equal("orders.place", op.Name);
            Assert.True(op.StartedAt <= DateTimeOffset.UtcNow);
            Assert.True(op.StartedAt > DateTimeOffset.UtcNow.AddMinutes(-1));

            op.SetAttribute("payment.method", "card");
            op.SetStatus(OperationStatus.Ok);
        }

        _provider.ForceFlush();

        var activity = Assert.Single(_exported);
        Assert.Equal("orders.place", activity.DisplayName);
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("ord-1", activity.GetTagItem("order.id")?.ToString());
        Assert.Equal(42L, Convert.ToInt64(activity.GetTagItem("user.id")));
        Assert.Equal("card", activity.GetTagItem("payment.method")?.ToString());
        Assert.True(activity.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public void RecordException_on_operation_shares_span_context()
    {
        var boom = new InvalidOperationException("payment declined");

        using (var op = _signals.StartOperation("orders.pay"))
        {
            op.RecordException(boom);
        }

        _provider.ForceFlush();

        var activity = Assert.Single(_exported);
        Assert.Equal("orders.pay", activity.DisplayName);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Contains(activity.Events, e => e.Name is "exception" or "ex");
    }

    [Fact]
    public void RecordException_without_active_operation_creates_error_span()
    {
        _signals.RecordException(
            new ArgumentException("bad input"),
            o => o.Attribute("reason", "validation"));

        _provider.ForceFlush();

        var activity = Assert.Single(_exported);
        Assert.Equal("exception", activity.DisplayName);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("validation", activity.GetTagItem("reason")?.ToString());
        Assert.Contains(activity.Events, e => e.Name is "exception" or "ex");
    }

    [Fact]
    public void RecordException_uses_current_activity_when_present()
    {
        using var outer = _signals.StartOperation("outer");
        _signals.RecordException(new Exception("nested"), o => o.Attribute("step", "2"));
        outer.Dispose();

        _provider.ForceFlush();

        var activity = Assert.Single(_exported);
        Assert.Equal("outer", activity.DisplayName);
        Assert.Equal("2", activity.GetTagItem("step")?.ToString());
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public void AddSignalGraph_registers_singleton()
    {
        var services = new ServiceCollection();
        services.AddSignalGraph(o => o.ServiceName = "from-di");

        using var sp = services.BuildServiceProvider();
        var a = sp.GetRequiredService<ISignalGraph>();
        var b = sp.GetRequiredService<ISignalGraph>();

        Assert.Same(a, b);
        Assert.IsType<SignalGraphTelemetry>(a);
    }

    public void Dispose()
    {
        _signals.Dispose();
        _provider.Dispose();
    }
}
