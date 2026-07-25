using Dilcore.Blueprints.Contracts.EntityDefinitions;
using FluentValidation.TestHelper;

namespace Dilcore.Blueprints.Contracts.Tests.EntityDefinitions;

[TestFixture]
public class CreateEntityReferenceDtoValidatorTests
{
    private CreateEntityReferenceDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntityReferenceDtoValidator();
    }

    [TestCase("OneToOne")]
    [TestCase("OneToMany")]
    [TestCase("ManyToOne")]
    [TestCase("onetoone")]
    [TestCase("ONETOMANY")]
    public void GivenValidation_WhenReferenceTypeIsValid_ThenShouldPass(string referenceType)
    {
        var dto = new CreateEntityReferenceDto
        {
            ReferenceType = referenceType,
            RelatedEntityDefinitionId = Guid.CreateVersion7()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ReferenceType);
    }

    [Test]
    public void GivenValidation_WhenReferenceTypeIsEmpty_ThenShouldHaveValidationError()
    {
        var dto = new CreateEntityReferenceDto
        {
            ReferenceType = "",
            RelatedEntityDefinitionId = Guid.CreateVersion7()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ReferenceType)
            .WithErrorMessage("ReferenceType is required.");
    }

    [Test]
    public void GivenValidation_WhenReferenceTypeIsInvalid_ThenShouldHaveValidationError()
    {
        var dto = new CreateEntityReferenceDto
        {
            ReferenceType = "ManyToMany",
            RelatedEntityDefinitionId = Guid.CreateVersion7()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ReferenceType)
            .WithErrorMessage("ReferenceType must be OneToOne, OneToMany, or ManyToOne.");
    }

    [Test]
    public void GivenValidation_WhenRelatedEntityDefinitionIdIsEmpty_ThenShouldHaveValidationError()
    {
        var dto = new CreateEntityReferenceDto
        {
            ReferenceType = "OneToOne",
            RelatedEntityDefinitionId = Guid.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RelatedEntityDefinitionId)
            .WithErrorMessage("RelatedEntityDefinitionId is required.");
    }

    [Test]
    public void GivenValidation_WhenAllFieldsValid_ThenShouldPass()
    {
        var dto = new CreateEntityReferenceDto
        {
            SchemaName = "customerOrder",
            ReferenceType = "OneToMany",
            RelatedEntityDefinitionId = Guid.CreateVersion7()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenValidation_WhenSchemaNameExceedsMaxLength_ThenShouldHaveValidationError()
    {
        var schemaName = "a" + new string('a', ValidationConstants.SchemaNameMaxLength);

        var dto = new CreateEntityReferenceDto
        {
            SchemaName = schemaName,
            ReferenceType = "OneToOne",
            RelatedEntityDefinitionId = Guid.CreateVersion7()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SchemaName)
            .WithErrorMessage($"SchemaName must not exceed {ValidationConstants.SchemaNameMaxLength} characters.");
    }
}
