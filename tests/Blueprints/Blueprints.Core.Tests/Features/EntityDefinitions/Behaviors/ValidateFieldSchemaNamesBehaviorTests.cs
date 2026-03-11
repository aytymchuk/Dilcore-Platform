using Dilcore.Blueprints.Core.Features.EntityDefinitions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Behaviors;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;
using Dilcore.Blueprints.Domain;
using Dilcore.Blueprints.Domain.Entities;
using FluentAssertions;
using FluentResults;
using MediatR;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions.Behaviors;

public class ValidateFieldSchemaNamesBehaviorTests
{
    private ValidateFieldSchemaNamesBehavior<CreateEntityDefinitionCommand> _createBehavior = null!;
    private ValidateFieldSchemaNamesBehavior<UpdateEntityDefinitionCommand> _updateBehavior = null!;
    private bool _nextCalled;

    private Task<Result<EntityDefinition>> Next(CancellationToken cancellationToken)
    {
        _nextCalled = true;
        return Task.FromResult(Result.Ok(new EntityDefinition { DisplayName = "Test" }));
    }

    [SetUp]
    public void Setup()
    {
        _createBehavior = new ValidateFieldSchemaNamesBehavior<CreateEntityDefinitionCommand>();
        _updateBehavior = new ValidateFieldSchemaNamesBehavior<UpdateEntityDefinitionCommand>();
        _nextCalled = false;
    }

    [Test]
    public async Task Handle_WhenFieldsHaveValidSchemaNames_ShouldCallNext()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields = [new FieldDefinitionParameters { DisplayName = "Name", Type = "string", SchemaName = "firstName" }]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenFieldSchemaNameIsNullOrEmpty_ShouldCallNext()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields =
            [
                new FieldDefinitionParameters { DisplayName = "Name", Type = "string", SchemaName = null },
                new FieldDefinitionParameters { DisplayName = "Age", Type = "number", SchemaName = "" },
                new FieldDefinitionParameters { DisplayName = "Email", Type = "string", SchemaName = "   " }
            ]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [TestCase("123schemaName")]
    [TestCase("123")]
    [TestCase("!!!")]
    public async Task Handle_WhenFieldSchemaNameNormalizesToInvalid_ShouldReturnValidationError(string schemaName)
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields = [new FieldDefinitionParameters { DisplayName = "Field", Type = "string", SchemaName = schemaName }]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain(schemaName);
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenFieldSchemaNameExceedsMaxLength_ShouldReturnValidationError()
    {
        var schemaName = "a" + new string('a', EntityDefinitionLimits.SchemaNameMaxLength);
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields = [new FieldDefinitionParameters { DisplayName = "Field", Type = "string", SchemaName = schemaName }]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _nextCalled.Should().BeFalse();
    }

    [TestCase("camelCase")]
    [TestCase("phone-number")]
    [TestCase("my-custom-schema")]
    public async Task Handle_WhenFieldSchemaNameIsNormalizableToValid_ShouldCallNext(string schemaName)
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields = [new FieldDefinitionParameters { DisplayName = "Field", Type = "string", SchemaName = schemaName }]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenNestedFieldHasInvalidSchemaName_ShouldReturnValidationError()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields =
            [
                new FieldDefinitionParameters
                {
                    DisplayName = "Address",
                    Type = "object",
                    SchemaName = "address",
                    Fields =
                    [
                        new FieldDefinitionParameters { DisplayName = "City", Type = "string", SchemaName = "123city" }
                    ]
                }
            ]
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("123city");
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenUpdateCommandHasInvalidFieldSchemaName_ShouldReturnValidationError()
    {
        var command = new UpdateEntityDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ETag = 1,
            Fields = [new FieldDefinitionParameters { DisplayName = "Field", Type = "string", SchemaName = "123invalid" }]
        };

        var result = await _updateBehavior.Handle(command, Next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("123invalid");
        _nextCalled.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenUpdateCommandHasValidFields_ShouldCallNext()
    {
        var command = new UpdateEntityDefinitionCommand
        {
            Id = Guid.NewGuid(),
            ETag = 1,
            Fields = [new FieldDefinitionParameters { DisplayName = "Name", Type = "string", SchemaName = "firstName" }]
        };

        var result = await _updateBehavior.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WhenFieldsAreEmpty_ShouldCallNext()
    {
        var command = new CreateEntityDefinitionCommand
        {
            DisplayName = "Test",
            Fields = []
        };

        var result = await _createBehavior.Handle(command, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextCalled.Should().BeTrue();
    }
}
