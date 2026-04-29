namespace Dilcore.Telemetry.Extensions.OpenTelemetry;

public record TelemetrySettings
{
    public string? ApplicationInsightsConnectionString { get; init; }

    /// <summary>
    /// Enables console metric export when Application Insights is not configured.
    /// Useful for local debugging, but can be noisy for Blazor Server (component metrics).
    /// </summary>
    public bool EnableLocalConsoleMetricsExporter { get; init; } = true;
}