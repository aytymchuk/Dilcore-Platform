using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Create;

public class CreateEntityDefinitionHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _grainMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private CreateEntityDefinitionHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _grainMock = new Mock<IEntityDefinitionGrain>();
        _mapperMock = new Mock<IMapper>();

        _sut = new CreateEntityDefinitionHandler(_grainFactoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_WhenDisplayNameIsEmpty_ShouldReturnValidationError()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "",
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>()
            .Which.Message.Should().Contain("DisplayName is required");
        _grainFactoryMock.Verify(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenDisplayNameIsWhitespace_ShouldReturnValidationError()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "   ",
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task Handle_WhenGrainSucceeds_ShouldReturnMappedEntity()
    {
        var grainId = Guid.CreateVersion7();
        var dto = new EntityDefinitionGrainDto
        {
            Id = grainId,
            SchemaName = "myEntity",
            DisplayName = "My Entity",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };
        var expectedEntity = new EntityDefinition(fields: null) { Id = grainId, SchemaName = "myEntity", DisplayName = "My Entity" };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Success(dto));

        _mapperMock
            .Setup(x => x.Map<CreateEntityDefinitionGrainCommand>(It.IsAny<CreateEntityDefinitionCommand>()))
            .Returns(new CreateEntityDefinitionGrainCommand { DisplayName = "My Entity", Fields = [] });
        _mapperMock
            .Setup(x => x.Map<EntityDefinition>(dto))
            .Returns(expectedEntity);

        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedEntity);
    }

    [Test]
    public async Task Handle_WhenGrainReturnsValidationError_ShouldMapToValidationError()
    {
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Validation("Invalid field type."));

        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>()
            .Which.Message.Should().Be("Invalid field type.");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsAlreadyExists_ShouldMapToConflictError()
    {
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateEntityDefinitionGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.AlreadyExists("Entity definition already exists."));

        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ConflictError>()
            .Which.Message.Should().Contain("already exists");
    }
}
