using AutoMapper;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.Blueprints.Store.Repositories;
using Dilcore.MongoDB.Abstractions;
using Dilcore.MongoDB.Abstractions.Repositories;
using Dilcore.MongoDB.Abstractions.Results;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Store.Tests;

[TestFixture]
public class EntityDefinitionRepositoryTests
{
    [Test]
    public async Task StoreAsync_WhenDuplicateKey_ReturnsConflictError()
    {
        var repository = new Mock<IGenericRepository<EntityDefinitionDocument>>();
        var mapper = new Mock<IMapper>();
        var tenantContextResolver = new Mock<ITenantContextResolver>();
        ITenantContext? tenantContext = null;
        tenantContextResolver.Setup(x => x.TryResolve(out tenantContext)).Returns(false);

        var entity = new EntityDefinition { DisplayName = "Customer", SchemaName = "customer" };
        mapper
            .Setup(x => x.Map<EntityDefinitionDocument>(entity))
            .Returns(new EntityDefinitionDocument
            {
                SchemaName = "customer",
                DisplayName = "Customer"
            });
        repository
            .Setup(x => x.StoreAsync(It.IsAny<EntityDefinitionDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<EntityDefinitionDocument>(new DuplicateKeyError()));

        var sut = new EntityDefinitionRepository(
            repository.Object,
            mapper.Object,
            tenantContextResolver.Object,
            Mock.Of<IMongoDbCollectionFactory>(),
            new BlueprintsIndexInitializer());

        var result = await sut.StoreAsync(entity);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>()
            .Which.Message.Should().Contain("customer");
    }
}
