using Dilcore.Blueprints.Store.Entities;
using Dilcore.DocumentDb.MongoDb.Extensions;
using Dilcore.DocumentDb.MongoDb.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.Store;

/// <summary>
/// MongoDB configuration extensions for Blueprints module.
/// </summary>
public static class MongoDbExtensions
{
    /// <summary>
    /// Database name for Blueprints module.
    /// </summary>
    public const string DatabaseName = "Blueprints";

    /// <summary>
    /// Collection name for Blueprints.
    /// </summary>
    public const string BlueprintsCollectionName = "blueprints";

    /// <summary>
    /// Adds Blueprints MongoDB configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">MongoDB settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlueprintsMongoDb(
        this IServiceCollection services,
        MongoDbSettings settings)
    {
        services.AddMongoDb(
            configure => configure.UseConnectionString(settings.ConnectionString),
            dbContainer =>
            {
                dbContainer.AddDatabase(DatabaseName, db =>
                {
                    db.AddGenericRepository<BlueprintDocument>(options =>
                    {
                        options.WithCollectionName(BlueprintsCollectionName);
                        options.WithDatabaseName(DatabaseName);
                    });
                });
            });

        return services;
    }
}
