using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions.Create;

public class CreateEntityDefinitionDtoValidatorTests
{
    private CreateEntityDefinitionDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntityDefinitionDtoValidator();
    }

    [TestCase("id")]
    [TestCase("createdAt")]
    [TestCase("tenantId")]
    public void GivenValidation_WhenSchemaNameIsReserved_ThenShouldHaveValidationError(string schemaName)
    {
        var dto = new CreateEntityDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "My Entity",
            Fields = [],
            Tags = []
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
            .WithErrorMessage($"SchemaName '{schemaName}' is reserved.");
    }

    [TestCase("myEntity")]
    [TestCase("validSchemaName")]
    public void GivenValidation_WhenSchemaNameFormatIsValid_ThenShouldPass(string schemaName)
    {
        var dto = new CreateEntityDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "My Entity",
            Fields = [],
            Tags = []
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }

    [TestCase("123invalid")]
    [TestCase("InvalidCaps")]
    [TestCase("phone-number")]
    public void GivenValidation_WhenSchemaNameFormatIsInvalid_ThenShouldHaveValidationError(string schemaName)
    {
        var dto = new CreateEntityDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "My Entity",
            Fields = [],
            Tags = []
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SchemaName);
    }

    [Test]
    public void GivenValidation_WhenSchemaNameExceedsMaxLength_ThenShouldHaveValidationError()
    {
        var schemaName = "a" + new string('a', ValidationConstants.SchemaNameMaxLength);

        var dto = new CreateEntityDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "My Entity",
            Fields = [],
            Tags = []
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
            .WithErrorMessage($"SchemaName must not exceed {ValidationConstants.SchemaNameMaxLength} characters.");
    }

    [Test]
    public void GivenValidation_WhenSchemaNameIsAtMaxLength_ThenShouldPass()
    {
        var schemaName = "a" + new string('a', ValidationConstants.SchemaNameMaxLength - 1);

        var dto = new CreateEntityDefinitionDto
        {
            SchemaName = schemaName,
            DisplayName = "My Entity",
            Fields = [],
            Tags = []
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.SchemaName);
    }
}
