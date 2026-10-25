namespace SignalGraph.Telemetry;

/// <summary>
/// Configures attributes and kind when starting an operation.
/// </summary>
public sealed class OperationBuilder
{
    internal List<KeyValuePair<string, object?>> Attributes { get; } = [];

    /// <summary>Attach a context attribute (why) before the operation starts.</summary>
    public OperationBuilder Attribute(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Attributes.Add(new KeyValuePair<string, object?>(key, value));
        return this;
    }
}
