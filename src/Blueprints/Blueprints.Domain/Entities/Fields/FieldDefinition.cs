namespace Dilcore.Blueprints.Domain.Entities.Fields;

public record FieldDefinition
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public virtual required FieldType Type { get; init; }
}
