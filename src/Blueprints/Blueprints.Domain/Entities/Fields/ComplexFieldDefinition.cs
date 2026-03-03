namespace Dilcore.Blueprints.Domain.Entities.Fields;

/// <summary>
/// Field definition for <see cref="FieldType.Object"/> and <see cref="FieldType.Array"/> types
/// that contain nested child fields.
/// </summary>
public record ComplexFieldDefinition : FieldDefinition
{
    public override required FieldType Type
    {
        get;
        init
        {
            if (value is not (FieldType.Object or FieldType.Array))
                throw new ArgumentException($"ComplexFieldDefinition only supports Object or Array types, got {value}.", nameof(Type));

            field = value;
        }
    }

    public IReadOnlyList<FieldDefinition> Fields { get; init; } = [];
}
