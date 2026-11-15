using Dilcore.Blueprints.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MultiTenant.MongoDb;
using Dilcore.MongoDB.DependencyInjection;
using MongoDB.Driver;

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
    /// Collection name for entity definitions.
    /// </summary>
    public const string EntityDefinitionsCollectionName = "entity_definitions";

    /// <summary>
    /// Adds the Blueprints database and entity definition document binding.
    /// The physical database name is prefixed with the current tenant storage identifier.
    /// </summary>
    /// <param name="builder">The MongoDB builder.</param>
    /// <param name="clusterName">The cluster that hosts the database.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMongoDbBuilder AddBlueprintsDatabase(this IMongoDbBuilder builder, string clusterName)
    {
        builder.AddDatabase(DatabaseName, database =>
        {
            database.OnCluster(clusterName);
            database.WithNamespacePrefixResolver<TenantDatabasePrefixResolver>();
            database.AddDocumentBinding<EntityDefinitionDocument>(EntityDefinitionsCollectionName, binding =>
            {
                binding.WithCollectionName(EntityDefinitionsCollectionName);
                binding.WithGuidIdGeneration(GuidIdGenerationStrategy.SequentialVersion7);
                binding.WithSoftDelete();
                binding.WithIndexes(CreateIndexes());
            });
        });

        return builder;
    }

    internal static CreateIndexModel<EntityDefinitionDocument>[] CreateIndexes()
    {
        return
        [
            new CreateIndexModel<EntityDefinitionDocument>(
                Builders<EntityDefinitionDocument>.IndexKeys.Ascending(x => x.SchemaName),
                new CreateIndexOptions<EntityDefinitionDocument>
                {
                    Name = "entity_definitions_schema_name",
                    Unique = true,
                    Collation = new Collation("en", strength: CollationStrength.Secondary),
                    PartialFilterExpression = Builders<EntityDefinitionDocument>.Filter.Eq(x => x.IsDeleted, false)
                }),
            new CreateIndexModel<EntityDefinitionDocument>(
                Builders<EntityDefinitionDocument>.IndexKeys.Ascending(x => x.DisplayName),
                new CreateIndexOptions
                {
                    Name = "entity_definitions_display_name"
                }),
            new CreateIndexModel<EntityDefinitionDocument>(
                Builders<EntityDefinitionDocument>.IndexKeys.Ascending(x => x.Metadata.Tags),
                new CreateIndexOptions
                {
                    Name = "entity_definitions_metadata_tags"
                })
        ];
    }
}
