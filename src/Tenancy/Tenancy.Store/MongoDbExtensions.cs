using Dilcore.DocumentDb.MongoDb.Extensions;
using Dilcore.DocumentDb.MongoDb.Repositories;
using Dilcore.Tenancy.Store.Entities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Dilcore.Tenancy.Store;

/// <summary>
/// MongoDB configuration extensions for Tenancy module.
/// </summary>
public static class MongoDbExtensions
{
    /// <summary>
    /// Database name for Tenancy module.
    /// </summary>
    public const string DatabaseName = "Tenancy";

    /// <summary>
    /// Collection name for Tenants.
    /// </summary>
    public const string TenantsCollectionName = "tenants";

    /// <summary>
    /// Adds Tenancy MongoDB configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">MongoDB settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTenancyMongoDb(
        this IServiceCollection services,
        MongoDbSettings settings)
    {
        services.AddMongoDb(
            configure => configure.UseConnectionString(settings.ConnectionString),
            dbContainer =>
            {
                dbContainer.AddDatabase(DatabaseName, db =>
                {
                    db.AddGenericRepository<TenantDocument>(options =>
                    {
                        var indexes = CreateIndexes().ToArray();

                        options.WithCollectionName(TenantsCollectionName);
                        options.WithDatabaseName(DatabaseName);
                        options.WithIndexes(indexes);
                    });
                });
            });

        return services;
    }

    private static IEnumerable<CreateIndexModel<TenantDocument>> CreateIndexes()
    {
        yield return new CreateIndexModel<TenantDocument>(
            Builders<TenantDocument>.IndexKeys.Ascending(x => x.SystemName), new CreateIndexOptions
            {
                Unique = true,
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            });
    }
}
