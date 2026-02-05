using Dilcore.Tenancy.Contracts.Tenants.Create;
using FluentValidation.TestHelper;

namespace Dilcore.Tenancy.Contracts.Tests.Tenants.Create;

[TestFixture]
public class CreateTenantDtoValidatorTests
{
    private CreateTenantDtoValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateTenantDtoValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new CreateTenantDto { Name = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Less_Than_2_Characters()
    {
        var model = new CreateTenantDto { Name = "a" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must be at least 2 characters");
    }

    [Test]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        var model = new CreateTenantDto { Name = new string('a', 101) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Test]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var model = new CreateTenantDto { Name = "Valid Name" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_When_Description_Exceeds_500_Characters()
    {
        var model = new CreateTenantDto 
        { 
            Name = "Valid Name", 
            Description = new string('a', 501) 
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Test]
    public void Should_Not_Have_Error_When_Description_Is_Null_Or_Empty()
    {
        var model1 = new CreateTenantDto { Name = "Valid Name", Description = null };
        var result1 = _validator.TestValidate(model1);
        result1.ShouldNotHaveValidationErrorFor(x => x.Description);

        var model2 = new CreateTenantDto { Name = "Valid Name", Description = string.Empty };
        var result2 = _validator.TestValidate(model2);
        result2.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_Not_Have_Error_When_Description_Is_Valid()
    {
        var model = new CreateTenantDto { Name = "Valid Name", Description = "Valid Description" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_Have_Error_When_Name_Does_Not_Contain_Alphanumeric_Characters()
    {
        var model = new CreateTenantDto { Name = "!!!" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must contain at least one alphanumeric character");
    }
}
