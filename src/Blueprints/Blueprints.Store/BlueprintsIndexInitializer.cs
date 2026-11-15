using System.Collections.Concurrent;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Keys;
using FluentResults;

namespace Dilcore.Blueprints.Store;

/// <summary>
/// Creates Blueprints indexes once per tenant storage identifier.
/// The collection factory must come from the current request scope so the tenant prefix resolves.
/// </summary>
public sealed class BlueprintsIndexInitializer
{
    private readonly ConcurrentDictionary<string, Task> _indexesByTenant = new(StringComparer.Ordinal);

    public async Task<Result> EnsureAsync(
        string storageIdentifier,
        IMongoDbCollectionFactory collectionFactory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageIdentifier))
        {
            return Result.Fail("Tenant storage identifier is not available.");
        }

        var creation = _indexesByTenant.GetOrAdd(
            storageIdentifier,
            _ => CreateIndexesAsync(collectionFactory));

        try
        {
            await creation.WaitAsync(cancellationToken);
            return Result.Ok();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            _indexesByTenant.TryRemove(new KeyValuePair<string, Task>(storageIdentifier, creation));
            return Result.Fail("Failed to create Blueprints indexes.");
        }
    }

    private static async Task CreateIndexesAsync(IMongoDbCollectionFactory collectionFactory)
    {
        var collection = await collectionFactory.GetCollectionAsync<EntityDefinitionDocument>(
            new MongoDocumentBindingKey(MongoDbExtensions.EntityDefinitionsCollectionName));

        if (collection.IsFailed)
        {
            var message = collection.Errors.Count > 0
                ? collection.Errors[0].Message
                : "Failed to resolve the entity definitions collection.";
            throw new InvalidOperationException(message);
        }

        await collection.Value.Indexes.CreateManyAsync(MongoDbExtensions.CreateIndexes());
    }
}
