using AutoMapper;
using Dilcore.Blueprints.Actors;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Actors.Storage;
using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Orleans;
using Shouldly;

namespace Dilcore.Blueprints.Actors.Tests.Storage;

[TestFixture]
[NonParallelizable]
public class BlueprintDefinitionStorageTests
{
    private ClusterFixture _clusterFixture = null!;
    private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
    private Mock<IServiceScope> _scopeMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IEntityDefinitionRepository> _repositoryMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private ILogger<BlueprintDefinitionStorage> _logger = null!;
    private BlueprintDefinitionStorage _storage = null!;
    private Mock<IGrainState<EntityDefinitionState>> _grainStateMock = null!;
    private readonly Guid _entityId = Guid.CreateVersion7();
    private GrainId _grainId;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _clusterFixture = new ClusterFixture();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _clusterFixture.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _grainId = _clusterFixture.Cluster.GrainFactory.GetGrain<IEntityDefinitionGrain>(_entityId).GetGrainId();

        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IEntityDefinitionRepository>();
        _mapperMock = new Mock<IMapper>();
        _logger = new LoggerFactory([NullLoggerProvider.Instance]).CreateLogger<BlueprintDefinitionStorage>();
        _grainStateMock = new Mock<IGrainState<EntityDefinitionState>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEntityDefinitionRepository))).Returns(_repositoryMock.Object);

        _storage = new BlueprintDefinitionStorage(_scopeFactoryMock.Object, _mapperMock.Object, _logger);
    }

    [Test]
    public async Task ReadStateAsync_ShouldLoadState_WhenEntityExists()
    {
        var entity = new EntityDefinition(fields: null)
        {
            Id = _entityId,
            ETag = 1,
            SchemaName = "myEntity",
            DisplayName = "My Entity"
        };
        var result = Result.Ok<EntityDefinition?>(entity);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var state = new EntityDefinitionState
        {
            Id = _entityId,
            ETag = 1,
            SchemaName = "myEntity",
            DisplayName = "My Entity"
        };
        _mapperMock.Setup(x => x.Map<EntityDefinitionState>(entity)).Returns(state);

        await _storage.ReadStateAsync("entityDefinition", _grainId, _grainStateMock.Object);

        _grainStateMock.VerifySet(x => x.State = state);
        _grainStateMock.VerifySet(x => x.RecordExists = true);
        _grainStateMock.VerifySet(x => x.ETag = "1");
    }

    [Test]
    public async Task ReadStateAsync_ShouldSetDefaultState_WhenEntityNotFound()
    {
        var result = Result.Ok<EntityDefinition?>(null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        await _storage.ReadStateAsync("entityDefinition", _grainId, _grainStateMock.Object);

        _grainStateMock.VerifySet(x => x.State = It.IsAny<EntityDefinitionState>());
        _grainStateMock.VerifySet(x => x.RecordExists = false);
        _grainStateMock.VerifySet(x => x.ETag = null);
    }

    [Test]
    public async Task ReadStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<EntityDefinition?>("DB error"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _storage.ReadStateAsync("entityDefinition", _grainId, _grainStateMock.Object));

        ex.Message.ShouldContain("Failed to read");
        ex.Message.ShouldContain("DB error");
    }

    [Test]
    public async Task WriteStateAsync_ShouldStoreEntity_WhenStateIsValid()
    {
        var state = new EntityDefinitionState
        {
            Id = _entityId,
            ETag = 1,
            SchemaName = "myEntity",
            DisplayName = "My Entity"
        };
        _grainStateMock.SetupGet(x => x.State).Returns(state);

        var entity = new EntityDefinition(fields: null)
        {
            Id = _entityId,
            ETag = 1,
            SchemaName = "myEntity",
            DisplayName = "My Entity"
        };
        _mapperMock.Setup(x => x.Map<EntityDefinition>(state)).Returns(entity);

        var storedEntity = entity with { ETag = 2 };
        var result = Result.Ok(storedEntity);

        _repositoryMock
            .Setup(x => x.StoreAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var updatedState = new EntityDefinitionState { Id = _entityId, ETag = 2, SchemaName = "myEntity", DisplayName = "My Entity" };
        _mapperMock.Setup(x => x.Map<EntityDefinitionState>(storedEntity)).Returns(updatedState);

        await _storage.WriteStateAsync("entityDefinition", _grainId, _grainStateMock.Object);

        _repositoryMock.Verify(x => x.StoreAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = true);
        _grainStateMock.VerifySet(x => x.ETag = "2");
    }

    [Test]
    public async Task WriteStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        var state = new EntityDefinitionState { Id = _entityId, SchemaName = "myEntity", DisplayName = "My Entity" };
        _grainStateMock.SetupGet(x => x.State).Returns(state);

        var entity = new EntityDefinition(fields: null) { Id = _entityId, SchemaName = "myEntity", DisplayName = "My Entity" };
        _mapperMock.Setup(x => x.Map<EntityDefinition>(state)).Returns(entity);

        _repositoryMock
            .Setup(x => x.StoreAsync(It.IsAny<EntityDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<EntityDefinition>("DB error"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _storage.WriteStateAsync("entityDefinition", _grainId, _grainStateMock.Object));

        ex.Message.ShouldContain("Failed to write");
        ex.Message.ShouldContain("DB error");
    }

    [Test]
    public async Task ClearStateAsync_ShouldDeleteEntity_WhenCalled()
    {
        _repositoryMock
            .Setup(x => x.DeleteAsync(_entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        await _storage.ClearStateAsync("entityDefinition", _grainId, _grainStateMock.Object);

        _repositoryMock.Verify(x => x.DeleteAsync(_entityId, It.IsAny<CancellationToken>()), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = false);
        _grainStateMock.VerifySet(x => x.ETag = null);
    }

    [Test]
    public async Task ClearStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        _repositoryMock
            .Setup(x => x.DeleteAsync(_entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("DB error"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _storage.ClearStateAsync("entityDefinition", _grainId, _grainStateMock.Object));

        ex.Message.ShouldContain("Failed to clear");
        ex.Message.ShouldContain("DB error");
    }

}
