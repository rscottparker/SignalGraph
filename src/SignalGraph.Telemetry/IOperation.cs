namespace SignalGraph.Telemetry;

/// <summary>
/// A timed unit of work (what/when) with optional causal context (why).
/// Dispose to end the operation and emit duration.
/// </summary>
public interface IOperation : IDisposable
{
    /// <summary>Operation name (what).</summary>
    string Name { get; }

    /// <summary>UTC start time (when).</summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>Add or overwrite a context attribute (why).</summary>
    IOperation SetAttribute(string key, object? value);

    /// <summary>Record an exception on this operation's span.</summary>
    IOperation RecordException(Exception exception, IEnumerable<KeyValuePair<string, object?>>? attributes = null);

    /// <summary>Set the final status of the operation.</summary>
    IOperation SetStatus(OperationStatus status, string? description = null);
}
