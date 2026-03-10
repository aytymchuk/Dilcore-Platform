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
}
