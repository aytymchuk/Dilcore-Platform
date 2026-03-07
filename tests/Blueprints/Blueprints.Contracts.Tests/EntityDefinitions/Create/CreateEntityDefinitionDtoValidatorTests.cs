using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions.Create;

public class CreateEntityDefinitionDtoValidatorTests
{
    private CreateEntityDefinitionDtoValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntityDefinitionDtoValidator();
    }

    [Test]
    public void GivenValidation_WhenSchemaNameIsNull_ThenShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateEntityDefinitionDto { SchemaName = null };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [Test]
    public void GivenValidation_WhenSchemaNameIsEmpty_ThenShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateEntityDefinitionDto { SchemaName = "" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [Test]
    public void GivenValidation_WhenSchemaNameIsWhitespace_ThenShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateEntityDefinitionDto { SchemaName = "   " };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [TestCase("camelCase")]
    [TestCase("camelCase012")]
    [TestCase("camel-case")]
    public void GivenValidation_WhenSchemaNameIsValid_ThenShouldNotHaveError(string schemaName)
    {
        // Arrange
        var dto = new CreateEntityDefinitionDto { SchemaName = schemaName };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [TestCase("InvalidName", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("PascalCase", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("schema_name", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("123schemaName", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("-schemaName", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("schemaName!", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    [TestCase("schema-Name$", "SchemaName must be camelCase, contain only latin characters, numbers, and hyphens.")]
    public void GivenValidation_WhenSchemaNameIsInvalidFormat_ThenShouldHaveValidationError(string schemaName, string expectedErrorMessage)
    {
        // Arrange
        var dto = new CreateEntityDefinitionDto { SchemaName = schemaName };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
              .WithErrorMessage(expectedErrorMessage);
    }

    [Test]
    public void GivenValidation_WhenSchemaNameExceedsMaxLength_ThenShouldHaveValidationError()
    {
        // Arrange
        var schemaName = "a" + new string('a', 64); // 65 chars
        var dto = new CreateEntityDefinitionDto { SchemaName = schemaName };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
              .WithErrorMessage("SchemaName must not exceed 64 characters.");
    }
}
