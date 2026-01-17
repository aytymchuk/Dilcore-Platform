using Dilcore.CorrelationId.Abstractions;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.CorrelationId.Http.Extensions;

public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds correlation ID tracking services to the service collection.
    /// </summary>
    public static IServiceCollection AddCorrelationIdTracking(this IServiceCollection services)
    {
        // Register middleware
        services.AddScoped<CorrelationIdMiddleware>();

        // Register HttpContextAccessor (required for provider)
        services.AddHttpContextAccessor();

        // Register providers and resolver as singletons (safe with IHttpContextAccessor)
        // HTTP provider has higher priority (100), Activity provider as fallback (50)
        services.AddSingleton<ICorrelationIdContextProvider, HttpCorrelationIdContextProvider>();
        services.AddSingleton<ICorrelationIdContextProvider, ActivityCorrelationIdContextProvider>();
        services.AddSingleton<ICorrelationIdContextResolver, CorrelationIdContextResolver>();

        // Register telemetry attribute provider for OTEL enrichment
        services.AddTelemetryAttributeProvider<CorrelationIdAttributeProvider>();

        return services;
    }

    /// <summary>
    /// Adds correlation ID middleware to the application pipeline.
    /// Should be added early in the pipeline to ensure correlation ID is available for all subsequent middleware.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
