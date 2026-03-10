using Dilcore.Blueprints.Contracts.EntityDefinitions;
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
