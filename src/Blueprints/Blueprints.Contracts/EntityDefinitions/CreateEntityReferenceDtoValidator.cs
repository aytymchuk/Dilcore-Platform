using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class CreateEntityReferenceDtoValidator : AbstractValidator<CreateEntityReferenceDto>
{
    private static readonly HashSet<string> ValidReferenceTypes =
        ["OneToOne", "OneToMany", "ManyToOne"];

    public CreateEntityReferenceDtoValidator()
    {
        RuleFor(x => x.SchemaName)
            .MaximumLength(ValidationConstants.SchemaNameMaxLength)
            .WithMessage($"SchemaName must not exceed {ValidationConstants.SchemaNameMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SchemaName));

        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .WithMessage("ReferenceType is required.")
            .Must(type => ValidReferenceTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("ReferenceType must be OneToOne, OneToMany, or ManyToOne.");

        RuleFor(x => x.RelatedEntityDefinitionId)
            .NotEqual(Guid.Empty)
            .WithMessage("RelatedEntityDefinitionId is required.");
    }
}
