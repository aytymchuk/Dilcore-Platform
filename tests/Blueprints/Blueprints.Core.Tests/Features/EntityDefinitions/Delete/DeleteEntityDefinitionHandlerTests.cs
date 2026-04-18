using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Delete;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Delete;

public class DeleteEntityDefinitionHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _grainMock = null!;
    private DeleteEntityDefinitionHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _grainMock = new Mock<IEntityDefinitionGrain>();

        _sut = new DeleteEntityDefinitionHandler(_grainFactoryMock.Object);
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
            .Setup(x => x.DeleteAsync())
            .ReturnsAsync(EntityDefinitionGrainResult.Success(dto));

        var command = new DeleteEntityDefinitionCommand(entityId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenGrainReturnsNotFound_ShouldMapToNotFoundError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.DeleteAsync())
            .ReturnsAsync(EntityDefinitionGrainResult.NotFound("Entity definition does not exist."));

        var command = new DeleteEntityDefinitionCommand(entityId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("does not exist");
    }
}
