using Dilcore.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Extension methods for registering multi-tenant services.
/// </summary>
public static class MultiTenantServiceCollectionExtensions
{
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