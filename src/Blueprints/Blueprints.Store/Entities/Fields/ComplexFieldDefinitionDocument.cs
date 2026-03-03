namespace Dilcore.Blueprints.Store.Entities.Fields;

/// <summary>
/// Document representation for <see cref="Domain.Entities.Fields.FieldType.Object"/>
/// and <see cref="Domain.Entities.Fields.FieldType.Array"/> types that contain nested child fields.
/// </summary>
public class ComplexFieldDefinitionDocument : FieldDefinitionDocument
{
    public List<FieldDefinitionDocument> Fields { get; set; } = [];
}
