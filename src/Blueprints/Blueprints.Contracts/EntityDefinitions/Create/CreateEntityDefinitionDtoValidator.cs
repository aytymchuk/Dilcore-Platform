using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Create;

public class CreateEntityDefinitionDtoValidator : AbstractValidator<CreateEntityDefinitionDto>
{
    private static readonly CreateEntityReferenceDtoValidator ReferenceValidator = new();

    public CreateEntityDefinitionDtoValidator()
    {
        RuleFor(x => x.SchemaName)
            .MaximumLength(ValidationConstants.SchemaNameMaxLength)
            .WithMessage($"SchemaName must not exceed {ValidationConstants.SchemaNameMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SchemaName));

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required.")
            .MinimumLength(ValidationConstants.DisplayNameMinLength)
            .WithMessage($"DisplayName must be at least {ValidationConstants.DisplayNameMinLength} characters.")
            .MaximumLength(ValidationConstants.DisplayNameMaxLength)
            .WithMessage($"DisplayName must not exceed {ValidationConstants.DisplayNameMaxLength} characters.")
            .Matches(ValidationConstants.AlphanumericRequiredPattern)
            .WithMessage("DisplayName must contain at least one alphanumeric character.");

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {ValidationConstants.DescriptionMaxLength} characters.");

        RuleFor(x => x.Fields.Count)
            .LessThanOrEqualTo(ValidationConstants.MaxTopLevelFields)
            .WithMessage($"An entity must not have more than {ValidationConstants.MaxTopLevelFields} top-level fields.");

        RuleForEach(x => x.Fields)
            .SetValidator(new FieldDefinitionDtoValidator());

        RuleFor(x => x.Tags.Count)
            .LessThanOrEqualTo(ValidationConstants.MaxTags)
            .WithMessage($"An entity must not have more than {ValidationConstants.MaxTags} tags.");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tags must not be empty.")
            .MaximumLength(ValidationConstants.MaxTagLength)
            .WithMessage($"Each tag must not exceed {ValidationConstants.MaxTagLength} characters.")
            .Matches(ValidationConstants.TagFormatPattern)
            .WithMessage("Tags must only contain alphanumeric characters, hyphens, or underscores.");

        RuleFor(x => x.References)
            .Must(references => references is null || references.Count <= ValidationConstants.MaxReferencesPerEntity)
            .WithMessage($"An entity must not have more than {ValidationConstants.MaxReferencesPerEntity} references.");

        When(x => x.References is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.References)
                .SetValidator(ReferenceValidator);
        });
    }
}
