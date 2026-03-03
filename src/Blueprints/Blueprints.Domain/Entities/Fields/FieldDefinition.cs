namespace Dilcore.Blueprints.Domain.Entities.Fields;

public record FieldDefinition
{
    public required string SchemaName { get; init; }
    public required string DisplayName { get; init; }
    public virtual required FieldType Type { get; init; }
}
