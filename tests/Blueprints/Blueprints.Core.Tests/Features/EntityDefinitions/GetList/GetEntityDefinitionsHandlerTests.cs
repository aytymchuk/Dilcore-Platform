using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;
using Dilcore.Blueprints.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Moq;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.GetList;

public class GetEntityDefinitionsHandlerTests
{
    private Mock<IEntityDefinitionRepository> _repositoryMock = null!;
    private GetEntityDefinitionsHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IEntityDefinitionRepository>();
        _sut = new GetEntityDefinitionsHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_WhenRepositorySucceeds_ShouldReturnPagedResult()
    {
        var items = new List<EntityDefinition>
        {
            new EntityDefinition(fields: null) { Id = Guid.CreateVersion7(), DisplayName = "Entity 1", SchemaName = "entity1" }
        };
        var totalCount = 1L;

        _repositoryMock
            .Setup(x => x.GetPagedAsync(0, 20, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<(IReadOnlyList<EntityDefinition>, long)>((items, totalCount)));

        var query = new GetEntityDefinitionsQuery { Skip = 0, Take = 20 };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task Handle_WhenRepositorySucceeds_ShouldPassThroughAllParameters()
    {
        var items = new List<EntityDefinition>();
        var tags = new List<string> { "crm" };

        _repositoryMock
            .Setup(x => x.GetPagedAsync(10, 5, "search", true, tags, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<(IReadOnlyList<EntityDefinition>, long)>((items, 0)));

        var query = new GetEntityDefinitionsQuery
        {
            Skip = 10,
            Take = 5,
            SearchTerm = "search",
            IsAbstract = true,
            Tags = tags
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.GetPagedAsync(10, 5, "search", true, tags, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenRepositoryFails_ShouldPropagateFailure()
    {
        _repositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<IReadOnlyList<string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<(IReadOnlyList<EntityDefinition>, long)>("Database error."));

        var query = new GetEntityDefinitionsQuery { Skip = 0, Take = 20 };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Database error.");
    }
}
