using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Update;

public class UpdateEntityDefinitionHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _grainMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private UpdateEntityDefinitionHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _grainMock = new Mock<IEntityDefinitionGrain>();
        _mapperMock = new Mock<IMapper>();

        _sut = new UpdateEntityDefinitionHandler(_grainFactoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_WhenGrainSucceeds_ShouldReturnMappedEntity()
    {
        var entityId = Guid.CreateVersion7();
        var dto = new EntityDefinitionGrainDto
        {
            Id = entityId,
            SchemaName = "myEntity",
            DisplayName = "My Entity",
            ETag = 2,
            Fields = [],
            References = [],
            Tags = []
        };
        var expectedEntity = new EntityDefinition(fields: null) { Id = entityId, SchemaName = "myEntity", DisplayName = "My Entity" };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Success(dto));

        _mapperMock
            .Setup(x => x.Map<UpdateEntityDefinitionGrainCommand>(It.IsAny<UpdateEntityDefinitionCommand>()))
            .Returns(new UpdateEntityDefinitionGrainCommand { ETag = 1, Fields = [] });
        _mapperMock
            .Setup(x => x.Map<EntityDefinition>(dto))
            .Returns(expectedEntity);

        var command = new UpdateEntityDefinitionCommand
        {
            Id = entityId,
            ETag = 1,
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedEntity);
    }

    [Test]
    public async Task Handle_WhenGrainReturnsNotFound_ShouldMapToNotFoundError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.NotFound("Entity definition does not exist."));

        _mapperMock
            .Setup(x => x.Map<UpdateEntityDefinitionGrainCommand>(It.IsAny<UpdateEntityDefinitionCommand>()))
            .Returns(new UpdateEntityDefinitionGrainCommand { ETag = 1, Fields = [] });

        var command = new UpdateEntityDefinitionCommand { Id = entityId, ETag = 1, Fields = [] };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("does not exist");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsETagMismatch_ShouldMapToConflictError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.ETagMismatch("ETag mismatch."));

        _mapperMock
            .Setup(x => x.Map<UpdateEntityDefinitionGrainCommand>(It.IsAny<UpdateEntityDefinitionCommand>()))
            .Returns(new UpdateEntityDefinitionGrainCommand { ETag = 1, Fields = [] });

        var command = new UpdateEntityDefinitionCommand { Id = entityId, ETag = 1, Fields = [] };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ConflictError>()
            .Which.Message.Should().Contain("ETag");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsValidationError_ShouldMapToValidationError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Validation("Invalid field type."));

        _mapperMock
            .Setup(x => x.Map<UpdateEntityDefinitionGrainCommand>(It.IsAny<UpdateEntityDefinitionCommand>()))
            .Returns(new UpdateEntityDefinitionGrainCommand { ETag = 1, Fields = [] });

        var command = new UpdateEntityDefinitionCommand { Id = entityId, ETag = 1, Fields = [] };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>()
            .Which.Message.Should().Be("Invalid field type.");
    }
}
