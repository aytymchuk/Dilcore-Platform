using FluentValidation;

namespace Dilcore.WebApi.Infrastructure.Validation;

/// <summary>
/// FluentValidation validator for ValidationDto demonstrating various validation rules.
/// </summary>
public sealed class ValidationDtoValidator : AbstractValidator<ValidationDto>
{
    public ValidationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 150).WithMessage("Age must be between 0 and 150.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g., +12345678901).");

        RuleFor(x => x.Website)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must be a valid URL.");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Length <= 10)
            .WithMessage("Tags must contain at most 10 items.");

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Each tag must not exceed 50 characters.")
            .When(x => x.Tags != null && x.Tags.Length > 0);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after Start date.");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
