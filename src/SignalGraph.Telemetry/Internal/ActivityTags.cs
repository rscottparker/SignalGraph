using System.Diagnostics;

namespace SignalGraph.Telemetry.Internal;

internal static class ActivityTags
{
    public static void Apply(Activity? activity, IEnumerable<KeyValuePair<string, object?>> attributes)
    {
        if (activity is null)
        {
            return;
        }

        foreach (var (key, value) in attributes)
        {
            activity.SetTag(key, value);
        }
    }

    public static void RecordException(Activity? activity, Exception exception, IEnumerable<KeyValuePair<string, object?>>? attributes = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (activity is null)
        {
            return;
        }

        if (attributes is not null)
        {
            Apply(activity, attributes);
        }

        activity.AddException(exception);
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
    }

    public static void ApplyStatus(Activity? activity, OperationStatus status, string? description)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(status switch
        {
            OperationStatus.Ok => ActivityStatusCode.Ok,
            OperationStatus.Error => ActivityStatusCode.Error,
            _ => ActivityStatusCode.Unset,
        }, description);
    }
}
