namespace Dilcore.Blueprints.Store.Entities;

public sealed class EntityReferenceDocument
{
    public required string SchemaName { get; set; }
    public required string ReferenceType { get; set; }
    public Guid RelatedEntityDefinitionId { get; set; }
    public required string RelatedEntitySchemaName { get; set; }
    public string? ReverseSchemaName { get; set; }
}
