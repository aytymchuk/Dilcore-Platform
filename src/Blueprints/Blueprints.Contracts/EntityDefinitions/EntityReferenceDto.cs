namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class EntityReferenceDto
{
    public string SchemaName { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public Guid RelatedEntityDefinitionId { get; set; }
    public string RelatedEntitySchemaName { get; set; } = string.Empty;
}
