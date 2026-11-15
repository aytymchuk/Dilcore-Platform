using Dilcore.Identity.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Keys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dilcore.Identity.Store;

internal sealed class UserCollectionIndexInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UserCollectionIndexInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var collectionFactory = scope.ServiceProvider.GetRequiredService<IMongoDbCollectionFactory>();
        var collection = await collectionFactory.GetCollectionAsync<UserDocument>(
            new MongoDocumentBindingKey(MongoDbExtensions.UsersCollectionName),
            cancellationToken);

        if (collection.IsFailed)
        {
            var message = collection.Errors.Count > 0
                ? collection.Errors[0].Message
                : "Failed to resolve the users collection.";
            throw new InvalidOperationException(message);
        }

        await collection.Value.Indexes.CreateManyAsync(MongoDbExtensions.CreateIndexes(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
