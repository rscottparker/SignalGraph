namespace SignalGraph.Telemetry;

/// <summary>
/// Outcome of an operation (maps to an OpenTelemetry span status).
/// </summary>
public enum OperationStatus
{
    /// <summary>Unset / default; treated as success when the operation ends without error.</summary>
    Unset = 0,

    /// <summary>Completed successfully.</summary>
    Ok = 1,

    /// <summary>Completed with an error.</summary>
    Error = 2,
}
