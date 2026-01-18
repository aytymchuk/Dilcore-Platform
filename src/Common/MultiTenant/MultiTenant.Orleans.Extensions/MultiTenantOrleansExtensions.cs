using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Dilcore.MultiTenant.Orleans.Extensions;

/// <summary>
/// Extension methods for integrating multi-tenancy with Orleans.
/// </summary>
public static class MultiTenantOrleansExtensions
{
    /// <summary>
    /// Adds Orleans tenant context support to the silo.
    /// This enables tenant context propagation across grain calls and makes ITenantContext
    /// available for injection in grain constructors.
    /// </summary>
    /// <param name="builder">The silo builder.</param>
    /// <returns>The silo builder for chaining.</returns>
    public static ISiloBuilder AddOrleansTenantContext(this ISiloBuilder builder)
    {
        // Register the Orleans tenant context provider
        builder.ConfigureServices(services =>
        {
            // Register tenant context provider (singleton, higher priority than HTTP provider)
            services.AddTenantContextProvider<OrleansTenantContextProvider>();
        });

        // Register incoming grain call filter to extract tenant context
        builder.AddIncomingGrainCallFilter<TenantIncomingGrainCallFilter>();

        // Register outgoing grain call filter to propagate tenant context
        builder.AddOutgoingGrainCallFilter<TenantOutgoingGrainCallFilter>();

        return builder;
    }

    /// <summary>
    /// Registers a tenant context provider as a singleton.
    /// </summary>
    /// <typeparam name="TProvider">The type of the tenant context provider to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenantContextProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, ITenantContextProvider
    {
        services.AddSingleton<ITenantContextProvider, TProvider>();
        return services;
    }
}
