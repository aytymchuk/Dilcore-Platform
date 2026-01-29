using Dilcore.WebApp.Models.Users;
using FluentValidation.TestHelper;

namespace Dilcore.WebApp.Tests.Models.Users;

[TestFixture]
public class RegisterUserParametersValidatorTests
{
    private RegisterUserParametersValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new RegisterUserParametersValidator();
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new RegisterUserParameters { Email = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new RegisterUserParameters { Email = "invalid-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        var model = new RegisterUserParameters { FirstName = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        var model = new RegisterUserParameters { LastName = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new RegisterUserParameters
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Have_Error_When_Email_Exceeds_Max_Length()
    {
        // Create email exceeding max length: local part (64 chars max per RFC) + domain to exceed total
        var localPart = new string('a', 64);
        var domainPart = new string('b', RegisterUserParametersValidator.MaxEmailLength - 64) + ".com";
        var model = new RegisterUserParameters { Email = $"{localPart}@{domainPart}" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_FirstName_Exceeds_Max_Length()
    {
        var model = new RegisterUserParameters { FirstName = new string('a', RegisterUserParametersValidator.MaxNameLength + 1) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Should_Have_Error_When_LastName_Exceeds_Max_Length()
    {
        var model = new RegisterUserParameters { LastName = new string('a', RegisterUserParametersValidator.MaxNameLength + 1) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
