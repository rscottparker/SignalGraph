namespace SignalGraph.Telemetry;

/// <summary>
/// Entry point for emitting operations (traces) and exceptions that share span context.
/// </summary>
public interface ISignalGraph
{
    /// <summary>
    /// Start a timed operation. Dispose the returned <see cref="IOperation"/> to stop timing.
    /// </summary>
    /// <param name="name">What happened (operation name).</param>
    /// <param name="configure">Optional attributes (why) applied at start.</param>
    IOperation StartOperation(string name, Action<OperationBuilder>? configure = null);

    /// <summary>
    /// Record an exception on the current operation, or on a short-lived error span if none is active.
    /// </summary>
    void RecordException(Exception exception, Action<OperationBuilder>? configure = null);
}
