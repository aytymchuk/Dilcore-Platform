using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;
using Dilcore.Blueprints.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Create.Behaviors;

public class ValidateExtendsEntityBehaviorTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _extendsGrainMock = null!;
    private ValidateExtendsEntityBehavior _sut = null!;
    private bool _nextCalled;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _extendsGrainMock = new Mock<IEntityDefinitionGrain>();
        _sut = new ValidateExtendsEntityBehavior(_grainFactoryMock.Object);
        _nextCalled = false;
    }

    [Test]
    public async Task Handle_WhenExtendsEntityIdIsNull_ShouldCallNextWithoutCallingGrain()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            ExtendsEntityId = null,
            Fields = []
        };

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
        _grainFactoryMock.Verify(x => x.GetGrain<IEntityDefinitionGrain>(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenExtendsEntityExists_ShouldCallNext()
    {
        var extendsId = Guid.CreateVersion7();
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Child Entity",
            ExtendsEntityId = extendsId,
            Fields = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(extendsId))
            .Returns(_extendsGrainMock.Object);

        _extendsGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(new EntityDefinitionGrainDto
            {
                Id = extendsId,
                SchemaName = "baseEntity",
                DisplayName = "Base Entity"
            });

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenExtendsEntityDoesNotExist_ShouldReturnValidationError()
    {
        var extendsId = Guid.CreateVersion7();
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Child Entity",
            ExtendsEntityId = extendsId,
            Fields = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(extendsId))
            .Returns(_extendsGrainMock.Object);

        _extendsGrainMock
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
        return Task.FromResult(Result.Ok(new EntityDefinition(fields: null) { DisplayName = "Test" }));
    }
}
