using FluentValidation;

namespace Dilcore.Identity.Contracts.Register;

/// <summary>
/// Validator for <see cref="RegisterUserDto"/>.
/// </summary>
public sealed class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    /// <summary>
    /// Maximum length for email address.
    /// </summary>
    public const int MaxEmailLength = 100;

    /// <summary>
    /// Maximum length for first name.
    /// </summary>
    public const int MaxFirstNameLength = 50;

    /// <summary>
    /// Maximum length for last name.
    /// </summary>
    public const int MaxLastNameLength = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserDtoValidator"/> class.
    /// </summary>
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(MaxEmailLength)
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(MaxFirstNameLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(MaxLastNameLength);
    }
}
