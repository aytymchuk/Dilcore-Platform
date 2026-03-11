using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class FieldDefinitionDtoValidator : AbstractValidator<FieldDefinitionDto>
{
    private readonly int _currentDepth;

    public FieldDefinitionDtoValidator() : this(1)
    {
    }

    internal FieldDefinitionDtoValidator(int depth)
    {
        _currentDepth = depth;

        RuleFor(x => x.SchemaName)
            .MaximumLength(ValidationConstants.SchemaNameMaxLength)
            .WithMessage($"Field schema name must not exceed {ValidationConstants.SchemaNameMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SchemaName));

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Field display name is required.")
            .MinimumLength(ValidationConstants.DisplayNameMinLength)
            .WithMessage($"Field display name must be at least {ValidationConstants.DisplayNameMinLength} characters.")
            .MaximumLength(ValidationConstants.DisplayNameMaxLength)
            .WithMessage($"Field display name must not exceed {ValidationConstants.DisplayNameMaxLength} characters.")
            .Matches(ValidationConstants.AlphanumericRequiredPattern)
            .WithMessage("Field display name must contain at least one alphanumeric character.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Field type is required.")
            .Must(FieldDefinitionDto.AllowedTypes.Contains)
            .WithMessage(x => $"Field type '{x.Type}' is not valid. Allowed: {string.Join(", ", FieldDefinitionDto.AllowedTypes)}.");

        RuleFor(x => x.Fields)
            .NotEmpty()
            .WithMessage("Object and Array fields must contain at least one nested field.")
            .When(x => x.Type is FieldDefinitionDto.TypeObject or FieldDefinitionDto.TypeArray);

        RuleFor(x => x.Fields)
            .Must(f => f is null || f.Count == 0)
            .WithMessage("Primitive field types must not have nested fields.")
            .When(x => x.Type is not (FieldDefinitionDto.TypeObject or FieldDefinitionDto.TypeArray));

        When(x => x.Fields is { Count: > 0 }, () =>
        {
            RuleFor(x => x.Fields!.Count)
                .LessThanOrEqualTo(ValidationConstants.MaxFieldsPerLevel)
                .WithMessage($"A field must not have more than {ValidationConstants.MaxFieldsPerLevel} nested fields.");

            RuleFor(x => x)
                .Must(_ => _currentDepth < ValidationConstants.MaxNestingDepth)
                .WithMessage($"Field nesting depth must not exceed {ValidationConstants.MaxNestingDepth} levels.");

            if (_currentDepth < ValidationConstants.MaxNestingDepth)
            {
                RuleForEach(x => x.Fields)
                    .SetValidator(new FieldDefinitionDtoValidator(_currentDepth + 1));
            }
        });
    }
}
