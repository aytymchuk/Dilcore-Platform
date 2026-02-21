using Dilcore.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.Store;

/// <summary>
/// Service collection extensions for Blueprints store configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Blueprints store services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlueprintsStore(
        this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetRequiredSettings<MongoDbSettings>();
        services.AddBlueprintsMongoDb(mongoDbSettings);
        return services;
    }
}
