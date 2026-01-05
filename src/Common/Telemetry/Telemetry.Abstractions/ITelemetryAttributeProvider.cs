namespace Dilcore.Telemetry.Abstractions;

/// <summary>
/// Provides custom attributes to be added to OpenTelemetry telemetry (logs, traces, metrics).
/// </summary>
public interface ITelemetryAttributeProvider
{
    /// <summary>
    /// Gets custom attributes to add to telemetry.
    /// </summary>
    /// <returns>A collection of key-value pairs to add as attributes. Returns empty collection if no attributes to add.</returns>
    IEnumerable<KeyValuePair<string, object?>> GetAttributes();
}
