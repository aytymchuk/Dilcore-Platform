using Dilcore.Blueprints.Contracts.EntityDefinitions;
using FluentValidation.TestHelper;
using Shouldly;

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

    [Test]
    public void GivenValidation_WhenFieldDtoIsValid_ThenShouldPass()
    {
        var dto = CreateValidFieldDto();

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void GivenValidation_WhenDisplayNameIsNullOrEmpty_ThenShouldHaveValidationError(string? displayName)
    {
        var dto = CreateValidFieldDto();
        dto.DisplayName = displayName!;

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [Test]
    public void GivenValidation_WhenDisplayNameIsTooShort_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.DisplayName = "A";

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [Test]
    public void GivenValidation_WhenDisplayNameExceedsMaxLength_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.DisplayName = new string('a', ValidationConstants.DisplayNameMaxLength + 1);

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [Test]
    public void GivenValidation_WhenDisplayNameHasNoAlphanumeric_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.DisplayName = "!!!@@@";

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DisplayName");
    }

    [TestCase("")]
    [TestCase("InvalidType")]
    [TestCase("unknown")]
    public void GivenValidation_WhenTypeIsInvalid_ThenShouldHaveValidationError(string type)
    {
        var dto = CreateValidFieldDto();
        dto.Type = type;

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Type");
    }

    [Test]
    public void GivenValidation_WhenObjectFieldHasNoNestedFields_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.Type = FieldDefinitionDto.TypeObject;
        dto.Fields = [];

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Fields");
    }

    [Test]
    public void GivenValidation_WhenPrimitiveFieldHasNestedFields_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.Type = FieldDefinitionDto.TypeString;
        dto.Fields = [CreateValidFieldDto("nested")];

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Fields");
    }

    [Test]
    public void GivenValidation_WhenNestingDepthExceedsLimit_ThenShouldHaveValidationError()
    {
        var dto = CreateValidFieldDto();
        dto.Type = FieldDefinitionDto.TypeObject;
        dto.Fields = [CreateNestedField(ValidationConstants.MaxNestingDepth + 1)];

        var result = _validator.Validate(dto);

        result.IsValid.ShouldBeFalse();
    }

    private static FieldDefinitionDto CreateNestedField(int depth)
    {
        if (depth <= 1)
        {
            var leaf = CreateValidFieldDto("leaf");
            leaf.Type = FieldDefinitionDto.TypeString;
            return leaf;
        }

        var field = CreateValidFieldDto($"level{depth}");
        field.Type = FieldDefinitionDto.TypeObject;
        field.Fields = [CreateNestedField(depth - 1)];
        return field;
    }
}
