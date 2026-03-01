using System.Text;
using System.Text.RegularExpressions;

namespace Dilcore.Blueprints.Domain;

/// <summary>
/// Generates camelCase schema names from display names, suitable for use
/// as SQL column names, MongoDB field names, and other storage identifiers.
/// Shared by <see cref="Entities.EntityDefinition"/> and <see cref="Entities.Fields.FieldDefinition"/>.
/// </summary>
public static partial class SchemaNameGenerator
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "eTag", "createdAt", "updatedAt", "isDeleted", "tenantId", "schemaName", "type"
    };

    public static string Generate(string displayName)
    {
        var words = WordSplitRegex().Split(displayName.Trim())
            .Where(w => w.Length > 0)
            .ToArray();

        if (words.Length == 0)
            return string.Empty;

        var sb = new StringBuilder(words[0].ToLowerInvariant());
        for (var i = 1; i < words.Length; i++)
        {
            sb.Append(char.ToUpperInvariant(words[i][0]));
            sb.Append(words[i][1..].ToLowerInvariant());
        }

        return sb.ToString();
    }

    public static bool IsReserved(string schemaName) =>
        ReservedNames.Contains(schemaName);

    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex WordSplitRegex();
}
