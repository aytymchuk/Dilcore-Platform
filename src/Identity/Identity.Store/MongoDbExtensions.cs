using Dilcore.DocumentDb.MongoDb.Extensions;
using Dilcore.DocumentDb.MongoDb.Repositories;
using Dilcore.Identity.Store.Entities;
using Microsoft.Extensions.DependencyInjection;
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
    /// Adds Identity MongoDB configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">MongoDB settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIdentityMongoDb(
        this IServiceCollection services,
        MongoDbSettings settings)
    {
        services.AddMongoDb(
            configure => configure.UseConnectionString(settings.ConnectionString),
            dbContainer =>
            {
                dbContainer.AddDatabase(DatabaseName, db =>
                {
                    db.AddGenericRepository<UserDocument>(options =>
                    {
                        var indexes = CreateIndexes().ToArray();

                        options.WithCollectionName(UsersCollectionName);
                        options.WithDatabaseName(DatabaseName);
                        options.WithIndexes(indexes);
                    });
                });
            });

        return services;
    }

    private static IEnumerable<CreateIndexModel<UserDocument>> CreateIndexes()
    {
        yield return new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(x => x.IdentityId), new CreateIndexOptions
            {
                Unique = true
            });

        yield return new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(x => x.Email), new CreateIndexOptions
            {
                Unique = true
            });
    }
}