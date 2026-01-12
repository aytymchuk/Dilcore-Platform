using Dilcore.Identity.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Identity.WebApi;

/// <summary>
/// Service collection extensions for Identity.WebApi dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Identity module services including Core and WebApi components.
    /// </summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        // Add Identity Core services (MediatR handlers and behaviors)
        services.AddIdentityApplication();

        return services;
    }
}
