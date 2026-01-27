using Dilcore.WebApp.Models.Users;
using FluentValidation.TestHelper;

namespace Dilcore.WebApp.Tests.Models.Users;

[TestFixture]
public class RegisterUserParametersValidatorTests
{
    private RegisterUserParametersValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new RegisterUserParametersValidator();
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new RegisterUserParameters { Email = "" };
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
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var model = new RegisterUserParameters { Email = "test@example.com" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        var model = new RegisterUserParameters { FirstName = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        var model = new RegisterUserParameters { LastName = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void Should_Pass_Validation_When_All_Fields_Are_Valid()
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
}
