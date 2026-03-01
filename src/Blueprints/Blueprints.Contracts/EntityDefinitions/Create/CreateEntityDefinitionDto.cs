namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Create;

public class CreateEntityDefinitionDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAbstract { get; set; }
    public Guid? ExtendsEntityId { get; set; }
    public List<FieldDefinitionDto> Fields { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}
