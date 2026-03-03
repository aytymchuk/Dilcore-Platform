namespace Dilcore.Blueprints.Contracts.EntityDefinitions.Update;

public class UpdateEntityDefinitionDto
{
    public long ETag { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsAbstract { get; set; }
    public List<FieldDefinitionDto> Fields { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}
