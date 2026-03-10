using System.Text.RegularExpressions;

namespace Dilcore.Blueprints.Domain;

public static partial class EntityDefinitionLimits
{
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 128;
    public const int DescriptionMaxLength = 200;

    public const int MaxTopLevelFields = 100;
    public const int MaxFieldsPerLevel = 50;
    public const int MaxNestingDepth = 5;

    public const int MaxTags = 20;
    public const int MaxTagLength = 64;

    public const int SchemaNameMaxLength = 64;

    public const string SchemaNameFormatPattern = @"^[a-z][a-zA-Z0-9]*$";

    private static readonly HashSet<string> ReservedSchemaNamesBacking = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "eTag", "createdAt", "updatedAt", "isDeleted", "tenantId", "schemaName", "type"
    };

    /// <summary>
    /// Reserved schema names used by the platform internally. Cannot be used as field schema names at any nesting depth.
    /// </summary>
    public static IReadOnlySet<string> ReservedSchemaNames => ReservedSchemaNamesBacking;

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    public static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    public static partial Regex TagFormatRegex();

    [GeneratedRegex(SchemaNameFormatPattern)]
    public static partial Regex SchemaNameFormatRegex();
}
