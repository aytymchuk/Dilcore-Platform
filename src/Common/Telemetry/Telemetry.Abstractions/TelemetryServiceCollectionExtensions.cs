using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Telemetry.Abstractions;

/// <summary>
/// Extension methods for registering telemetry attribute providers.
/// </summary>
public static class TelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Registers a telemetry attribute provider as a singleton.
    /// </summary>
    /// <typeparam name="TProvider">The type of the attribute provider to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTelemetryAttributeProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, ITelemetryAttributeProvider
    {
        services.AddSingleton<ITelemetryAttributeProvider, TProvider>();
        return services;
    }
}