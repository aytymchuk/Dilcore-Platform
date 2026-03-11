namespace Dilcore.Blueprints.Domain.Entities;

public record EntityReference
{
    public required string SchemaName { get; init; }
    public required EntityReferenceType ReferenceType { get; init; }
    public required Guid RelatedEntityDefinitionId { get; init; }
    public required string RelatedEntitySchemaName { get; init; }
}
