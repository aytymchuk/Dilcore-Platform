namespace Dilcore.OpenTelemetry.Extensions;

public record TelemetrySettings
{
    public string? ApplicationInsightsConnectionString { get; init; }
}