# SignalGraph.Telemetry

API-first C# library for apps and APIs that need enough telemetry for **traces** and **exceptions**. Callers answer **what** happened, **when** it ran, and **why** (business / causal context). Spans and exceptions share OpenTelemetry context.

Target framework: **net8.0**.

## Quickstart

```bash
dotnet add package SignalGraph.Telemetry
# until published: reference src/SignalGraph.Telemetry/SignalGraph.Telemetry.csproj
```

### Register

```csharp
builder.Services.AddSignalGraph(o =>
{
    o.ServiceName = "orders-api";
    o.ServiceVersion = "1.0.0";
});

// Host-owned OTel export — add your exporter of choice:
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddSource("orders-api")
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());
```

### Emit an operation

```csharp
public sealed class CheckoutService(ISignalGraph signals)
{
    public async Task PlaceAsync(string orderId, string userId)
    {
        using var op = signals.StartOperation("orders.place", o => o
            .Attribute("order.id", orderId)
            .Attribute("user.id", userId));

        try
        {
            // work…
            op.SetStatus(OperationStatus.Ok);
        }
        catch (Exception ex)
        {
            op.RecordException(ex);
            throw;
        }
    }
}
```

### ASP.NET request spans (optional)

```csharp
using SignalGraph.Telemetry.AspNetCore;

app.UseSignalGraphRequestTelemetry();
```

## What / when / why → OpenTelemetry

| Concept | API | OTel |
| --- | --- | --- |
| What | operation name | span / `Activity` name |
| When | start + dispose | activity timestamps / duration |
| Why | `Attribute(...)` | activity tags |
| Errors | `RecordException` | exception on the same span |

## Layout

```
src/SignalGraph.Telemetry/     library
tests/SignalGraph.Telemetry.Tests/
```

## Develop

```bash
dotnet test
```
