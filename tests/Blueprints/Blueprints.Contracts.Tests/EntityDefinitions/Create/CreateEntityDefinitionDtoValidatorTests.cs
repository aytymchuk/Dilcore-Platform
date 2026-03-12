using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions.Create;

[TestFixture]
public class CreateEntityDefinitionDtoValidatorTests
{
    private CreateEntityDefinitionDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntityDefinitionDtoValidator();
    }

    [TestCase("myEntity")]
    [TestCase("validSchemaName")]
    [TestCase("phone-number")]
    [TestCase("my-entity-suffix")]
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
