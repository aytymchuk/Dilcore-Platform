using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Update;

public class UpdateEntityDefinitionDtoValidator : AbstractValidator<UpdateEntityDefinitionDto>
{
    public UpdateEntityDefinitionDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName must not be empty.")
            .MinimumLength(ValidationConstants.DisplayNameMinLength)
            .WithMessage($"DisplayName must be at least {ValidationConstants.DisplayNameMinLength} characters.")
            .MaximumLength(ValidationConstants.DisplayNameMaxLength)
            .WithMessage($"DisplayName must not exceed {ValidationConstants.DisplayNameMaxLength} characters.")
            .Matches(ValidationConstants.AlphanumericRequiredPattern)
            .WithMessage("DisplayName must contain at least one alphanumeric character.")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {ValidationConstants.DescriptionMaxLength} characters.");

        When(x => x.Fields is { Count: > 0 }, () =>
        {
            RuleFor(x => x.Fields!.Count)
                .LessThanOrEqualTo(ValidationConstants.MaxTopLevelFields)
                .WithMessage($"An entity definition must not have more than {ValidationConstants.MaxTopLevelFields} top-level fields.");

            RuleForEach(x => x.Fields)
                .SetValidator(new FieldDefinitionDtoValidator(1));
        });

        When(x => x.Tags is not null, () =>
        {
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
        });
    }
}
