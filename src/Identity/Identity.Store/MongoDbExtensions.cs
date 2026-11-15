using Dilcore.Identity.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.DependencyInjection;
using MongoDB.Driver;

namespace Dilcore.Identity.Store;

/// <summary>
/// MongoDB configuration extensions for Identity module.
/// </summary>
public static class MongoDbExtensions
{
    /// <summary>
    /// Database name for Identity module.
    /// </summary>
    public const string DatabaseName = "Identity";

    /// <summary>
    /// Collection name for Users.
    /// </summary>
    public const string UsersCollectionName = "users";

    /// <summary>
    /// Adds the Identity database and user document binding.
    /// </summary>
    /// <param name="builder">The MongoDB builder.</param>
    /// <param name="clusterName">The cluster that hosts the database.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMongoDbBuilder AddIdentityDatabase(this IMongoDbBuilder builder, string clusterName)
    {
        builder.AddDatabase(DatabaseName, database =>
        {
            database.OnCluster(clusterName);
            database.AddDocumentBinding<UserDocument>(UsersCollectionName, binding =>
            {
                binding.WithCollectionName(UsersCollectionName);
                binding.WithGuidIdGeneration(GuidIdGenerationStrategy.SequentialVersion7);
                binding.WithSoftDelete();
                binding.WithIndexes(CreateIndexes());
            });
        });

        return builder;
    }

    internal static CreateIndexModel<UserDocument>[] CreateIndexes()
    {
        return
        [
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.IdentityId),
                new CreateIndexOptions<UserDocument>
                {
                    Name = "users_identity_id",
                    Unique = true,
                    PartialFilterExpression = Builders<UserDocument>.Filter.Eq(x => x.IsDeleted, false)
                }),
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(x => x.Email),
                new CreateIndexOptions<UserDocument>
                {
                    Name = "users_email",
                    Unique = true,
                    Collation = new Collation("en", strength: CollationStrength.Secondary),
                    PartialFilterExpression = Builders<UserDocument>.Filter.Eq(x => x.IsDeleted, false)
                })
        ];
    }
}
