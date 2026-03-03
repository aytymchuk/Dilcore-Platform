using MongoDB.Bson.Serialization.Attributes;

namespace Dilcore.Blueprints.Store.Entities.Fields;

[BsonKnownTypes(typeof(ComplexFieldDefinitionDocument))]
public class FieldDefinitionDocument
{
    public required string SchemaName { get; set; }
    public required string DisplayName { get; set; }
    public required string Type { get; set; }
}
