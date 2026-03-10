using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;
using Dilcore.Blueprints.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Moq;
using MediatR;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Create.Behaviors;

public class UniqueSchemaNameBehaviorTests
{
    private Mock<IEntityDefinitionRepository> _repositoryMock = null!;
    private UniqueSchemaNameBehavior _sut = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IEntityDefinitionRepository>();
        _sut = new UniqueSchemaNameBehavior(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_WhenDisplayNameHasNoAlphanumeric_ShouldReturnValidationErrorWithoutCallingRepository()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "!!!",
            Fields = []
        };

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("alphanumeric");
        _repositoryMock.Verify(x => x.ExistsBySchemaNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Task<Result<EntityDefinition>> Next(CancellationToken _) =>
        Task.FromResult(Result.Ok(new EntityDefinition { DisplayName = "Test" }));
}
