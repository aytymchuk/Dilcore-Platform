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
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: "ValidFirst",
            LastName: "ValidLast");

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
            FirstName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxFirstNameLength)),
            LastName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxLastNameLength)));

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
            FirstName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxFirstNameLength)),
            LastName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxLastNameLength)));

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
            FirstName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxFirstNameLength)),
            LastName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxLastNameLength)));

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WithEmptyFirstName_ShouldFail()
    {
        // Arrange
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: string.Empty,
            LastName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxLastNameLength)));

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Validate_WithFirstNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: new string('a', RegisterUserDtoValidator.MaxFirstNameLength + 1),
            LastName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxLastNameLength)));

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Validate_WithEmptyLastName_ShouldFail()
    {
        // Arrange
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxFirstNameLength)),
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
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxFirstNameLength)),
            LastName: new string('a', RegisterUserDtoValidator.MaxLastNameLength + 1));

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    [TestCase("John123")]
    [TestCase("John!")]
    [TestCase("John@")]
    public void Validate_WithInvalidCharactersInFirstName_ShouldFail(string firstName)
    {
        // Arrange
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: firstName,
            LastName: "ValidLast");

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First Name contains invalid characters.");
    }

    [Test]
    [TestCase("Doe123")]
    [TestCase("Doe!")]
    [TestCase("Doe@")]
    public void Validate_WithInvalidCharactersInLastName_ShouldFail(string lastName)
    {
        // Arrange
        const string domain = "@example.com";
        var localPart = _fixture.Create<string>().Substring(0, Math.Min(10, RegisterUserDtoValidator.MaxEmailLength - domain.Length));
        var dto = new RegisterUserDto(
            Email: localPart + domain,
            FirstName: "ValidFirst",
            LastName: lastName);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last Name contains invalid characters.");
    }
}
