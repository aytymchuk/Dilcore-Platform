using Dilcore.Configuration.Extensions;
using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Extensions;

public static class SettingsExtensions
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterConfiguration<ApplicationSettings>(configuration);
        services.RegisterConfiguration<AuthenticationSettings>(configuration);
        // TelemetrySettings is registered by AddTelemetry if using generic? 
        // Or maybe it was here. 
        // If TelemetrySettings is in OpenTelemetry.Extensions, usage of RegisterConfiguration<TelemetrySettings> requires reference.
        // WebApi references OpenTelemetry.Extensions.
        // But AddTelemetry (in OpenTelemetry.Extensions) might register it?
        // Let's check how AddTelemetry works. It takes IConfiguration.
        // In step 114 snippet: services.AddSingleton(sp => configuration.GetSettings<TelemetrySettings>()); or similar.
        // So AddTelemetry handles its own settings.

        return services;
    }
}