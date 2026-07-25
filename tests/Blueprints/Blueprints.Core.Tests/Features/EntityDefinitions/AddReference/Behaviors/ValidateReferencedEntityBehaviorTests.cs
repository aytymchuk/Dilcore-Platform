using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference.Behaviors;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.AddReference.Behaviors;

public class ValidateReferencedEntityBehaviorTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _relatedGrainMock = null!;
    private ValidateReferencedEntityBehavior _sut = null!;
    private bool _nextCalled;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _relatedGrainMock = new Mock<IEntityDefinitionGrain>();
        _sut = new ValidateReferencedEntityBehavior(_grainFactoryMock.Object);
        _nextCalled = false;
    }

    [Test]
    public async Task Handle_WhenReferencedEntityExists_ShouldCallNext()
    {
        var relatedId = Guid.CreateVersion7();
        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = Guid.CreateVersion7(),
            ReferenceType = EntityReferenceType.OneToOne,
            RelatedEntityDefinitionId = relatedId
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_relatedGrainMock.Object);

        _relatedGrainMock
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
        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = Guid.CreateVersion7(),
            ReferenceType = EntityReferenceType.OneToMany,
            RelatedEntityDefinitionId = relatedId
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_relatedGrainMock.Object);

        _relatedGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((EntityDefinitionGrainDto?)null);

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("does not exist");
        _nextCalled.Should().BeFalse();
    }

    private Task<Result<EntityDefinition>> Next(CancellationToken _)
    {
        _nextCalled = true;
        return Task.FromResult(Result.Ok(new EntityDefinition(fields: null, references: null) { DisplayName = "Test" }));
    }
}
