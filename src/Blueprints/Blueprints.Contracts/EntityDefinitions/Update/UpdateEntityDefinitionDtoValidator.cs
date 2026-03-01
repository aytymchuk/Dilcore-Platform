using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Update;

public class UpdateEntityDefinitionDtoValidator : AbstractValidator<UpdateEntityDefinitionDto>
{
    private static readonly FieldDefinitionDtoValidator FieldValidator = new();

    public UpdateEntityDefinitionDtoValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description must not exceed 200 characters.");

        RuleForEach(x => x.Fields)
            .SetValidator(FieldValidator);
    }
}
