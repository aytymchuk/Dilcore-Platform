namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

public class FieldDefinitionDto
{
    public const string TypeString = "String";
    public const string TypeNumber = "Number";
    public const string TypeBoolean = "Boolean";
    public const string TypeDateTime = "DateTime";
    public const string TypeObject = "Object";
    public const string TypeArray = "Array";
    public const string TypeFile = "File";
    public const string TypeIdentifier = "Identifier";

    public static readonly IReadOnlySet<string> AllowedTypes = new HashSet<string>
    {
        TypeString, TypeNumber, TypeBoolean, TypeDateTime,
        TypeObject, TypeArray, TypeFile, TypeIdentifier
    };

    public string SchemaName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<FieldDefinitionDto>? Fields { get; set; }
}
