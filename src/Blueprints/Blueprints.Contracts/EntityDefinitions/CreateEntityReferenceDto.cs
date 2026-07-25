namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class CreateEntityReferenceDto
{
    public string? SchemaName { get; set; }
    public required string ReferenceType { get; set; }
    public required Guid RelatedEntityDefinitionId { get; set; }
}
