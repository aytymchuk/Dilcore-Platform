using Dilcore.Telemetry.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dilcore.WebApi.Extensions;

public static class TelemetryEnricherExtensions
{
    public static IServiceCollection AddTelemetryEnricher<T>(this IServiceCollection services)
        where T : class, ITelemetryEnricher
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITelemetryEnricher, T>());
        return services;
    }
}