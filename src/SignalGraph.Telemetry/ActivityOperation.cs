using System.Diagnostics;
using SignalGraph.Telemetry.Internal;

namespace SignalGraph.Telemetry;

internal sealed class ActivityOperation : IOperation
{
    private readonly Activity? _activity;
    private bool _disposed;

    public ActivityOperation(string name, Activity? activity)
    {
        Name = name;
        _activity = activity;
        StartedAt = activity?.StartTimeUtc is { Ticks: > 0 } utc
            ? new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc))
            : DateTimeOffset.UtcNow;
    }

    public string Name { get; }

    public DateTimeOffset StartedAt { get; }

    public IOperation SetAttribute(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _activity?.SetTag(key, value);
        return this;
    }

    public IOperation RecordException(Exception exception, IEnumerable<KeyValuePair<string, object?>>? attributes = null)
    {
        ActivityTags.RecordException(_activity, exception, attributes);
        return this;
    }

    public IOperation SetStatus(OperationStatus status, string? description = null)
    {
        ActivityTags.ApplyStatus(_activity, status, description);
        return this;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_activity is null)
        {
            return;
        }

        if (_activity.Status == ActivityStatusCode.Unset)
        {
            _activity.SetStatus(ActivityStatusCode.Ok);
        }

        _activity.Stop();
        _activity.Dispose();
    }
}
