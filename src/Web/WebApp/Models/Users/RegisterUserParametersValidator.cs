using FluentValidation;

namespace Dilcore.WebApp.Models.Users;

/// <summary>
/// FluentValidation validator for RegisterUserParameters.
/// </summary>
public sealed class RegisterUserParametersValidator : AbstractValidator<RegisterUserParameters>
{
    /// <summary>
    /// Maximum length for email address.
    /// </summary>
    public const int MaxEmailLength = 256;

    /// <summary>
    /// Maximum length for first and last names.
    /// </summary>
    public const int MaxNameLength = 100;

    public RegisterUserParametersValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please enter a valid email address")
            .MaximumLength(MaxEmailLength).WithMessage($"Email cannot exceed {MaxEmailLength} characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(MaxNameLength).WithMessage($"First name cannot exceed {MaxNameLength} characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(MaxNameLength).WithMessage($"Last name cannot exceed {MaxNameLength} characters");
    }
}
