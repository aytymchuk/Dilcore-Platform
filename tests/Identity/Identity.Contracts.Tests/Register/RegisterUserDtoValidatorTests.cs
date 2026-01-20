using AutoFixture;
using Dilcore.Identity.Contracts.Register;
using FluentValidation.TestHelper;

namespace Dilcore.Identity.Contracts.Tests.Register;

[TestFixture]
public sealed class RegisterUserDtoValidatorTests
{
    private RegisterUserDtoValidator _validator = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterUserDtoValidator();
        _fixture = new Fixture();
    }

    [Test]
    public void Validate_WithValidInput_ShouldPass()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: _fixture.Create<string>() + "@example.com",
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: string.Empty,
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WithInvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: "invalid-email",
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WithEmailExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var email = new string('a', RegisterUserDtoValidator.MaxEmailLength + 1) + "@example.com";
        var dto = new RegisterUserDto(
            Email: email,
            FirstName: _fixture.Create<string>(),
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WithEmptyFirstName_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: _fixture.Create<string>() + "@example.com",
            FirstName: string.Empty,
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Validate_WithFirstNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: _fixture.Create<string>() + "@example.com",
            FirstName: new string('a', RegisterUserDtoValidator.MaxFirstNameLength + 1),
            LastName: _fixture.Create<string>());

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Validate_WithEmptyLastName_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: _fixture.Create<string>() + "@example.com",
            FirstName: _fixture.Create<string>(),
            LastName: string.Empty);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void Validate_WithLastNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto(
            Email: _fixture.Create<string>() + "@example.com",
            FirstName: _fixture.Create<string>(),
            LastName: new string('a', RegisterUserDtoValidator.MaxLastNameLength + 1));

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
