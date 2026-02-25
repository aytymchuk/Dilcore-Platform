namespace Dilcore.Blueprints.Store.Entities.Fields;

public class FieldDefinitionDocument
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Type { get; set; }

    public List<FieldDefinitionDocument>? Fields { get; set; }
}
