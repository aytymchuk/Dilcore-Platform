namespace Dilcore.Telemetry.Extensions.OpenTelemetry;

public record TelemetrySettings
{
    public string? ApplicationInsightsConnectionString { get; init; }
}