using Dilcore.Tenancy.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Tenancy.WebApi;

/// <summary>
/// Service collection extensions for Tenancy.WebApi dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Tenancy module services including Core and WebApi components.
    /// </summary>
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        // Add Tenancy Core services (MediatR handlers and behaviors)
        services.AddTenancyApplication();

        return services;
    }
}
