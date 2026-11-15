using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Store.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Identity.Store;

/// <summary>
/// Service collection extensions for Identity store configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity store services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddIdentityStore();
    /// </code>
    /// </example>
    public static IServiceCollection AddIdentityStore(this IServiceCollection services)
    {
        services.AddHostedService<UserCollectionIndexInitializer>();

        services.AddAutoMapper(opts => opts.AddMaps(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
