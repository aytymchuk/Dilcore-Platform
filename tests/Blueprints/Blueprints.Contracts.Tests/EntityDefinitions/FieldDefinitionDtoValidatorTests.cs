using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions;

public class FieldDefinitionDtoValidatorTests
{
    private FieldDefinitionDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new FieldDefinitionDtoValidator();
    }

    [Test]
    public void GivenValidation_WhenSchemaNameIsEmpty_ThenShouldNotHaveError()
    {
        // Arrange
        var dto = CreateValidFieldDto(schemaName: "");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [TestCase("camelCase")]
    [TestCase("camelCase012")]
    [TestCase("camel-case")]
    [TestCase("firstName")]
    [TestCase("a")]
    [TestCase("x123")]
    public void GivenValidation_WhenFieldSchemaNameIsValid_ThenShouldNotHaveError(string schemaName)
    {
        // Arrange
        var dto = CreateValidFieldDto(schemaName: schemaName);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [TestCase("PascalCase")]
    [TestCase("schema_name")]
    [TestCase("123schemaName")]
    [TestCase("-schemaName")]
    [TestCase("schemaName!")]
    [TestCase("schema-Name$")]
    public void GivenValidation_WhenFieldSchemaNameIsInvalidFormat_ThenShouldHaveValidationError(string schemaName)
    {
        // Arrange
        var dto = CreateValidFieldDto(schemaName: schemaName);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
              .WithErrorMessage("Field schema name must be camelCase, contain only latin characters, numbers, and hyphens.");
    }

    [Test]
    public void GivenValidation_WhenFieldSchemaNameExceedsMaxLength_ThenShouldHaveValidationError()
    {
        // Arrange
        var schemaName = "a" + new string('a', 64); // 65 chars
        var dto = CreateValidFieldDto(schemaName: schemaName);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
              .WithErrorMessage("Field schema name must not exceed 64 characters.");
    }

    [Test]
    public void GivenValidation_WhenEntityDefinitionHasFieldWithValidSchemaName_ThenShouldNotHaveError()
    {
        // Arrange
        var entityValidator = new CreateEntityDefinitionDtoValidator();
        var dto = new CreateEntityDefinitionDto
        {
            DisplayName = "TestEntity",
            Fields =
            [
                CreateValidFieldDto(schemaName: "firstName"),
                CreateValidFieldDto(schemaName: "lastName")
            ]
        };

        // Act
        var result = entityValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Fields[0].SchemaName");
        result.ShouldNotHaveValidationErrorFor("Fields[1].SchemaName");
    }

    [Test]
    public void GivenValidation_WhenEntityDefinitionHasFieldWithInvalidSchemaName_ThenShouldHaveError()
    {
        // Arrange
        var entityValidator = new CreateEntityDefinitionDtoValidator();
        var dto = new CreateEntityDefinitionDto
        {
            DisplayName = "TestEntity",
            Fields =
            [
                CreateValidFieldDto(schemaName: "InvalidName!")
            ]
        };

        // Act
        var result = entityValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor("Fields[0].SchemaName");
    }

    private static FieldDefinitionDto CreateValidFieldDto(string schemaName = "validName")
    {
        return new FieldDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "Valid Field",
            Type = FieldDefinitionDto.TypeString
        };
    }
}
