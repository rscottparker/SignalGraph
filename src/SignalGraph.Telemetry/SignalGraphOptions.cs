namespace SignalGraph.Telemetry;

/// <summary>
/// Options for <see cref="ISignalGraph"/>.
/// </summary>
public sealed class SignalGraphOptions
{
    /// <summary>
    /// Logical service name used as the <see cref="System.Diagnostics.ActivitySource"/> name
    /// (and typically mirrored as OTel <c>service.name</c> by the host's tracer provider).
    /// </summary>
    public string ServiceName { get; set; } = "SignalGraph";

    /// <summary>
    /// Optional version string attached as the ActivitySource version.
    /// </summary>
    public string? ServiceVersion { get; set; }
}
