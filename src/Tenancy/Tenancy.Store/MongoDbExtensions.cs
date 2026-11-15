using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.DependencyInjection;
using Dilcore.Tenancy.Store.Entities;
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
    /// Adds the Tenancy database and tenant document binding.
    /// </summary>
    /// <param name="builder">The MongoDB builder.</param>
    /// <param name="clusterName">The cluster that hosts the database.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMongoDbBuilder AddTenancyDatabase(this IMongoDbBuilder builder, string clusterName)
    {
        builder.AddDatabase(DatabaseName, database =>
        {
            database.OnCluster(clusterName);
            database.AddDocumentBinding<TenantDocument>(TenantsCollectionName, binding =>
            {
                binding.WithCollectionName(TenantsCollectionName);
                binding.WithGuidIdGeneration(GuidIdGenerationStrategy.SequentialVersion7);
                binding.WithSoftDelete();
                binding.WithIndexes(CreateIndexes());
            });
        });

        return builder;
    }

    internal static CreateIndexModel<TenantDocument>[] CreateIndexes()
    {
        return
        [
            new CreateIndexModel<TenantDocument>(
                Builders<TenantDocument>.IndexKeys.Ascending(x => x.SystemName),
                new CreateIndexOptions<TenantDocument>
                {
                    Name = "tenants_system_name",
                    Unique = true,
                    Collation = new Collation("en", strength: CollationStrength.Secondary),
                    PartialFilterExpression = Builders<TenantDocument>.Filter.Eq(x => x.IsDeleted, false)
                })
        ];
    }
}
