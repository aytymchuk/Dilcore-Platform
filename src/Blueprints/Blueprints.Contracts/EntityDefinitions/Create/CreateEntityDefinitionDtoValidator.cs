using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Create;

public class CreateEntityDefinitionDtoValidator : AbstractValidator<CreateEntityDefinitionDto>
{
    private static readonly FieldDefinitionDtoValidator FieldValidator = new();

    public CreateEntityDefinitionDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required.")
            .MinimumLength(2)
            .WithMessage("DisplayName must be at least 2 characters.")
            .MaximumLength(128)
            .WithMessage("DisplayName must not exceed 128 characters.")
            .Matches(@"[a-zA-Z0-9]")
            .WithMessage("DisplayName must contain at least one alphanumeric character.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description must not exceed 200 characters.");

        RuleForEach(x => x.Fields)
            .SetValidator(FieldValidator);
    }
}
