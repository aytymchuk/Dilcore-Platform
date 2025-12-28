namespace Dilcore.WebApi.Settings;

public record TelemetrySettings
{
    public string? ApplicationInsightsConnectionString { get; init; }
}