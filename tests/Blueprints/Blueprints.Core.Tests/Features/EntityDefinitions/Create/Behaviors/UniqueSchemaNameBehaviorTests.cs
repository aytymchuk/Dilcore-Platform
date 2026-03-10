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
    private bool _nextCalled;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IEntityDefinitionRepository>();
        _sut = new UniqueSchemaNameBehavior(_repositoryMock.Object);
        _nextCalled = false;
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

    [Test]
    public async Task Handle_WhenSchemaNameDoesNotExist_ShouldCallNextAndInvokeRepositoryWithGeneratedSchemaName()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };
        _repositoryMock
            .Setup(x => x.ExistsBySchemaNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(false));

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.ExistsBySchemaNameAsync("myEntity", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenSchemaNameAlreadyExists_ShouldReturnConflictErrorWithoutCallingNext()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };
        _repositoryMock
            .Setup(x => x.ExistsBySchemaNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(true));

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("already exists");
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "My Entity",
            Fields = []
        };
        var expectedMessage = "Database connection failed";
        _repositoryMock
            .Setup(x => x.ExistsBySchemaNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(expectedMessage));

        var act = () => _sut.Handle(command, Next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*" + expectedMessage + "*");
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenSchemaNameIsReserved_ShouldReturnValidationErrorWithoutCallingRepository()
    {
        var command = new CreateEntityDefinitionCommand
        {
            SchemaName = "id",
            DisplayName = "My Entity",
            Fields = []
        };

        var result = await _sut.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("cannot be normalized");
        _repositoryMock.Verify(x => x.ExistsBySchemaNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private Task<Result<EntityDefinition>> Next(CancellationToken _)
    {
        _nextCalled = true;
        return Task.FromResult(Result.Ok(new EntityDefinition { DisplayName = "Test" }));
    }
}
