using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;
using Dilcore.Blueprints.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Create.Behaviors;

public class ValidateCreateEntityReferencesBehaviorTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _targetGrainMock = null!;
    private ValidateCreateEntityReferencesBehavior _sut = null!;
    private bool _nextCalled;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _targetGrainMock = new Mock<IEntityDefinitionGrain>();
        _sut = new ValidateCreateEntityReferencesBehavior(_grainFactoryMock.Object);
        _nextCalled = false;
    }

    [Test]
    public async Task Handle_WhenReferencesIsNull_ShouldCallNextWithoutCallingGrain()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            References = null!,
            Fields = []
        };

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
        _grainFactoryMock.Verify(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenReferencesIsEmpty_ShouldCallNextWithoutCallingGrain()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            References = [],
            Fields = []
        };

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
        _grainFactoryMock.Verify(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenAllReferencedEntitiesExist_ShouldCallNext()
    {
        var relatedId = Guid.CreateVersion7();
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            References =
            [
                new EntityReferenceParameters
                {
                    RelatedEntityDefinitionId = relatedId,
                    ReferenceType = EntityReferenceType.OneToOne
                }
            ],
            Fields = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new EntityDefinitionGrainDto
            {
                Id = relatedId,
                SchemaName = "customer",
                DisplayName = "Customer"
            });

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenReferencedEntityDoesNotExist_ShouldReturnValidationError()
    {
        var relatedId = Guid.CreateVersion7();
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            References =
            [
                new EntityReferenceParameters
                {
                    RelatedEntityDefinitionId = relatedId,
                    ReferenceType = EntityReferenceType.OneToMany
                }
            ],
            Fields = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((EntityDefinitionGrainDto?)null);

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("does not exist");
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenMultipleReferencesAndOneDoesNotExist_ShouldReturnValidationError()
    {
        var existingId = Guid.CreateVersion7();
        var missingId = Guid.CreateVersion7();
        var existingGrainMock = new Mock<IEntityDefinitionGrain>();

        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            References =
            [
                new EntityReferenceParameters
                {
                    RelatedEntityDefinitionId = existingId,
                    ReferenceType = EntityReferenceType.OneToOne
                },
                new EntityReferenceParameters
                {
                    RelatedEntityDefinitionId = missingId,
                    ReferenceType = EntityReferenceType.ManyToOne
                }
            ],
            Fields = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(existingId))
            .Returns(existingGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(missingId))
            .Returns(_targetGrainMock.Object);

        existingGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new EntityDefinitionGrainDto
            {
                Id = existingId,
                SchemaName = "existing",
                DisplayName = "Existing"
            });
        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((EntityDefinitionGrainDto?)null);

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain(missingId.ToString());
        _nextCalled.Should().BeFalse();
    }

    private Task<Result<EntityDefinition>> Next(CancellationToken _)
    {
        _nextCalled = true;
        return Task.FromResult(Result.Ok(new EntityDefinition(fields: null) { DisplayName = "Test" }));
    }
}
