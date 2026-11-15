using Dilcore.Blueprints.Store;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Keys;
using FluentAssertions;
using FluentResults;
using MongoDB.Driver;
using Moq;

namespace Dilcore.Blueprints.Store.Tests;

[TestFixture]
public class BlueprintsIndexInitializerTests
{
    [Test]
    public async Task EnsureAsync_SameStorageIdentifier_CreatesIndexesOnce()
    {
        var factory = CreateFactory();
        var sut = new BlueprintsIndexInitializer();

        var first = await sut.EnsureAsync("tenant-a", factory.Object, CancellationToken.None);
        var second = await sut.EnsureAsync("tenant-a", factory.Object, CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        factory.Verify(
            x => x.GetCollectionAsync<EntityDefinitionDocument>(
                It.IsAny<MongoDocumentBindingKey>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EnsureAsync_DifferentStorageIdentifiers_CreatesIndexesPerTenant()
    {
        var factory = CreateFactory();
        var sut = new BlueprintsIndexInitializer();

        await sut.EnsureAsync("tenant-a", factory.Object, CancellationToken.None);
        await sut.EnsureAsync("tenant-b", factory.Object, CancellationToken.None);

        factory.Verify(
            x => x.GetCollectionAsync<EntityDefinitionDocument>(
                It.IsAny<MongoDocumentBindingKey>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private static Mock<IMongoDbCollectionFactory> CreateFactory()
    {
        var indexes = new Mock<IMongoIndexManager<EntityDefinitionDocument>>();
        indexes
            .Setup(x => x.CreateManyAsync(
                It.IsAny<IEnumerable<CreateIndexModel<EntityDefinitionDocument>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(["entity_definitions_schema_name"]);

        var collection = new Mock<IMongoCollection<EntityDefinitionDocument>>();
        collection.SetupGet(x => x.Indexes).Returns(indexes.Object);

        var factory = new Mock<IMongoDbCollectionFactory>();
        factory
            .Setup(x => x.GetCollectionAsync<EntityDefinitionDocument>(
                It.IsAny<MongoDocumentBindingKey>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(collection.Object));

        return factory;
    }
}
