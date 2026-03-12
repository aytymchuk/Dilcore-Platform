using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.RemoveReference;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.RemoveReference;

public class RemoveEntityReferenceHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _grainMock = null!;
    private RemoveEntityReferenceHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _grainMock = new Mock<IEntityDefinitionGrain>();

        _sut = new RemoveEntityReferenceHandler(_grainFactoryMock.Object);
    }

    [Test]
    public async Task Handle_WhenGrainSucceeds_ShouldReturnSuccess()
    {
        var entityId = Guid.CreateVersion7();
        var dto = new EntityDefinitionGrainDto
        {
            Id = entityId,
            SchemaName = "myEntity",
            DisplayName = "My Entity",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.RemoveReferenceAsync(It.Is<RemoveEntityReferenceGrainCommand>(c => c.SchemaName == "customer")))
            .ReturnsAsync(EntityDefinitionGrainResult.Success(dto, new EntityReferenceGrainDto
            {
                SchemaName = "customer",
                ReferenceType = "OneToOne",
                RelatedEntityDefinitionId = Guid.CreateVersion7(),
                RelatedEntitySchemaName = "customer"
            }));

        var command = new RemoveEntityReferenceCommand(entityId, "customer");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenGivenNonCanonicalSchema_ShouldNormalizeAndReturnSuccess()
    {
        var entityId = Guid.CreateVersion7();
        var dto = new EntityDefinitionGrainDto
        {
            Id = entityId,
            SchemaName = "myEntity",
            DisplayName = "My Entity",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.RemoveReferenceAsync(It.Is<RemoveEntityReferenceGrainCommand>(c => c.SchemaName == "customer")))
            .ReturnsAsync(EntityDefinitionGrainResult.Success(dto, new EntityReferenceGrainDto
            {
                SchemaName = "customer",
                ReferenceType = "OneToOne",
                RelatedEntityDefinitionId = Guid.CreateVersion7(),
                RelatedEntitySchemaName = "customer"
            }));

        var command = new RemoveEntityReferenceCommand(entityId, "Customer ");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _grainMock.Verify(
            x => x.RemoveReferenceAsync(It.Is<RemoveEntityReferenceGrainCommand>(c => c.SchemaName == "customer")),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenGrainReturnsNotFound_ShouldMapToNotFoundError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.RemoveReferenceAsync(It.IsAny<RemoveEntityReferenceGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.NotFound("Reference does not exist."));

        var command = new RemoveEntityReferenceCommand(entityId, "customer");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("does not exist");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsValidation_ShouldMapToValidationError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.RemoveReferenceAsync(It.IsAny<RemoveEntityReferenceGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Validation("Reverse reference removal failed."));

        var command = new RemoveEntityReferenceCommand(entityId, "customer");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>()
            .Which.Message.Should().Contain("Reverse reference removal failed");
    }
}
