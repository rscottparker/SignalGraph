using System.Diagnostics;
using Microsoft.Extensions.Options;
using SignalGraph.Telemetry.Internal;

namespace SignalGraph.Telemetry;

/// <summary>
/// Default <see cref="ISignalGraph"/> implementation backed by <see cref="ActivitySource"/>.
/// </summary>
public sealed class SignalGraphTelemetry : ISignalGraph, IDisposable
{
    private readonly ActivitySource _source;
    private bool _disposed;

    public SignalGraphTelemetry(IOptions<SignalGraphOptions> options)
        : this(options.Value)
    {
    }

    public SignalGraphTelemetry(SignalGraphOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ServiceName);

        _source = string.IsNullOrWhiteSpace(options.ServiceVersion)
            ? new ActivitySource(options.ServiceName)
            : new ActivitySource(options.ServiceName, options.ServiceVersion);
    }

    /// <summary>ActivitySource used for operations (useful for test listeners / OTel registration).</summary>
    public ActivitySource ActivitySource => _source;

    public IOperation StartOperation(string name, Action<OperationBuilder>? configure = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var builder = new OperationBuilder();
        configure?.Invoke(builder);

        var activity = _source.StartActivity(name, ActivityKind.Internal);
        ActivityTags.Apply(activity, builder.Attributes);

        return new ActivityOperation(name, activity);
    }

    public void RecordException(Exception exception, Action<OperationBuilder>? configure = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(exception);

        var builder = new OperationBuilder();
        configure?.Invoke(builder);

        var current = Activity.Current;
        if (current is not null)
        {
            ActivityTags.Apply(current, builder.Attributes);
            ActivityTags.RecordException(current, exception);
            return;
        }

        using var activity = _source.StartActivity("exception", ActivityKind.Internal);
        ActivityTags.Apply(activity, builder.Attributes);
        ActivityTags.RecordException(activity, exception);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _source.Dispose();
    }
}
