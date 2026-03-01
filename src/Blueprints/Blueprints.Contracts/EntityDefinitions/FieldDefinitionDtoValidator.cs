using FluentValidation;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class FieldDefinitionDtoValidator : AbstractValidator<FieldDefinitionDto>
{
    public FieldDefinitionDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Field display name is required.")
            .MinimumLength(2)
            .WithMessage("Field display name must be at least 2 characters.")
            .MaximumLength(128)
            .WithMessage("Field display name must not exceed 128 characters.")
            .Matches(@"[a-zA-Z0-9]")
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

        RuleForEach(x => x.Fields)
            .SetValidator(this!)
            .When(x => x.Fields is { Count: > 0 });
    }
}
