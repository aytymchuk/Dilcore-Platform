using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Store.Repositories;
using Dilcore.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
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
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddIdentityStore(configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddIdentityStore(
        this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetRequiredSettings<MongoDbSettings>();
        // Configure MongoDB
        services.AddIdentityMongoDb(mongoDbSettings);

        // Register AutoMapper profiles from this assembly
        services.AddAutoMapper(opts => opts.AddMaps(typeof(ServiceCollectionExtensions).Assembly));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}