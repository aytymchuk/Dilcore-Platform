using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.AddReference;

public class AddEntityReferenceHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IEntityDefinitionGrain> _sourceGrainMock = null!;
    private Mock<IEntityDefinitionGrain> _targetGrainMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private AddEntityReferenceHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _sourceGrainMock = new Mock<IEntityDefinitionGrain>();
        _targetGrainMock = new Mock<IEntityDefinitionGrain>();
        _mapperMock = new Mock<IMapper>();

        _sut = new AddEntityReferenceHandler(_grainFactoryMock.Object, _mapperMock.Object);
    }

    [Test]
    public async Task Handle_WhenGrainSucceeds_ShouldReturnMappedEntity()
    {
        var entityId = Guid.CreateVersion7();
        var relatedId = Guid.CreateVersion7();
        var targetDto = new EntityDefinitionGrainDto
        {
            Id = relatedId,
            SchemaName = "customer",
            DisplayName = "Customer",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };
        var updatedDto = new EntityDefinitionGrainDto
        {
            Id = entityId,
            SchemaName = "order",
            DisplayName = "Order",
            ETag = 2,
            Fields = [],
            References = [new EntityReferenceGrainDto { SchemaName = "customer", ReferenceType = "OneToOne", RelatedEntityDefinitionId = relatedId, RelatedEntitySchemaName = "customer" }],
            Tags = []
        };
        var expectedEntity = new EntityDefinition(fields: null) { Id = entityId, SchemaName = "order", DisplayName = "Order" };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_sourceGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(targetDto);

        _sourceGrainMock
            .Setup(x => x.AddReferenceAsync(It.IsAny<AddEntityReferenceGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Success(updatedDto));

        _mapperMock
            .Setup(x => x.Map<EntityDefinition>(updatedDto))
            .Returns(expectedEntity);

        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = entityId,
            RelatedEntityDefinitionId = relatedId,
            ReferenceType = EntityReferenceType.OneToOne
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedEntity);
    }

    [Test]
    public async Task Handle_WhenGrainReturnsNotFound_ShouldMapToNotFoundError()
    {
        var entityId = Guid.CreateVersion7();
        var relatedId = Guid.CreateVersion7();
        var targetDto = new EntityDefinitionGrainDto
        {
            Id = relatedId,
            SchemaName = "customer",
            DisplayName = "Customer",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_sourceGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(targetDto);

        _sourceGrainMock
            .Setup(x => x.AddReferenceAsync(It.IsAny<AddEntityReferenceGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.NotFound("Entity definition not found."));

        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = entityId,
            RelatedEntityDefinitionId = relatedId,
            ReferenceType = EntityReferenceType.OneToOne
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("not found");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsValidationError_ShouldMapToValidationError()
    {
        var entityId = Guid.CreateVersion7();
        var relatedId = Guid.CreateVersion7();
        var targetDto = new EntityDefinitionGrainDto
        {
            Id = relatedId,
            SchemaName = "customer",
            DisplayName = "Customer",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_sourceGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(targetDto);

        _sourceGrainMock
            .Setup(x => x.AddReferenceAsync(It.IsAny<AddEntityReferenceGrainCommand>()))
            .ReturnsAsync(EntityDefinitionGrainResult.Validation("Reference limit exceeded."));

        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = entityId,
            RelatedEntityDefinitionId = relatedId,
            ReferenceType = EntityReferenceType.OneToOne
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>()
            .Which.Message.Should().Be("Reference limit exceeded.");
    }

    [Test]
    public async Task Handle_WhenGrainReturnsSuccessWithoutEntity_ShouldReturnUnexpectedError()
    {
        var entityId = Guid.CreateVersion7();
        var relatedId = Guid.CreateVersion7();
        var targetDto = new EntityDefinitionGrainDto
        {
            Id = relatedId,
            SchemaName = "customer",
            DisplayName = "Customer",
            ETag = 1,
            Fields = [],
            References = [],
            Tags = []
        };

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_sourceGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(targetDto);

        _sourceGrainMock
            .Setup(x => x.AddReferenceAsync(It.IsAny<AddEntityReferenceGrainCommand>()))
            .ReturnsAsync(new EntityDefinitionGrainResult
            {
                IsSuccess = true
            });

        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = entityId,
            RelatedEntityDefinitionId = relatedId,
            ReferenceType = EntityReferenceType.OneToOne
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnexpectedError>()
            .Which.Message.Should().Contain("without entity");
    }

    [Test]
    public async Task Handle_WhenTargetIsDeletedConcurrently_ShouldReturnNotFoundError()
    {
        var entityId = Guid.CreateVersion7();
        var relatedId = Guid.CreateVersion7();

        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(entityId))
            .Returns(_sourceGrainMock.Object);
        _grainFactoryMock
            .Setup(x => x.GetGrain<IEntityDefinitionGrain>(relatedId))
            .Returns(_targetGrainMock.Object);

        _targetGrainMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync((EntityDefinitionGrainDto?)null);

        var command = new AddEntityReferenceCommand
        {
            EntityDefinitionId = entityId,
            RelatedEntityDefinitionId = relatedId,
            ReferenceType = EntityReferenceType.OneToOne
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Contain("Referenced entity definition not found");
    }
}
