namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class EntityDefinitionDto
{
    public Guid Id { get; set; }
    public long ETag { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAbstract { get; set; }
    public Guid? ExtendsEntityId { get; set; }
    public List<FieldDefinitionDto> Fields { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
