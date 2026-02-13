using FluentValidation;

namespace Dilcore.WebApp.Models.Tenants;

/// <summary>
/// Validator for CreateTenantParameters.
/// </summary>
public class CreateTenantParametersValidator : AbstractValidator<CreateTenantParameters>
{
    public CreateTenantParametersValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters")
            .Matches(@"[a-zA-Z0-9]")
            .WithMessage("Name must contain at least one alphanumeric character");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
