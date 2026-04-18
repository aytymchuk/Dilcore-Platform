using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.GetById;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.GetById;

public class GetEntityDefinitionHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _grainMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetEntityDefinitionHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _grainMock = new Mock<IEntityDefinitionGrain>();
        _mapperMock = new Mock<IMapper>();

        _sut = new GetEntityDefinitionHandler(_grainFactoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_WhenEntityExists_ShouldReturnMappedEntity()
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
        var expectedEntity = new EntityDefinition(fields: null) { Id = entityId, SchemaName = "myEntity", DisplayName = "My Entity" };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(dto);

        _mapperMock
            .Setup(x => x.Map<EntityDefinition>(dto))
            .Returns(expectedEntity);

        var query = new GetEntityDefinitionQuery(entityId);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedEntity);
    }

    [Test]
    public async Task Handle_WhenEntityDoesNotExist_ShouldReturnNotFoundError()
    {
        var entityId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_grainMock.Object);

        _grainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((EntityDefinitionGrainDto?)null);

        var query = new GetEntityDefinitionQuery(entityId);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("not found");
    }
}
