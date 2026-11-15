using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Keys;
using Dilcore.Tenancy.Store.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dilcore.Tenancy.Store;

internal sealed class TenantCollectionIndexInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TenantCollectionIndexInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var collectionFactory = scope.ServiceProvider.GetRequiredService<IMongoDbCollectionFactory>();
        var collection = await collectionFactory.GetCollectionAsync<TenantDocument>(
            new MongoDocumentBindingKey(MongoDbExtensions.TenantsCollectionName),
            cancellationToken);

        if (collection.IsFailed)
        {
            var message = collection.Errors.Count > 0
                ? collection.Errors[0].Message
                : "Failed to resolve the tenants collection.";
            throw new InvalidOperationException(message);
        }

        await collection.Value.Indexes.CreateManyAsync(MongoDbExtensions.CreateIndexes(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
