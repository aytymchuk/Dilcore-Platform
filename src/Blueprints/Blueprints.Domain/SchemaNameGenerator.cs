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
    private static readonly Regex FormatRegex = EntityDefinitionLimits.SchemaNameFormatRegex();

    public static string Generate(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be null or empty.", nameof(displayName));
        }

        var words = EntityDefinitionLimits.NonAlphanumericRegex()
            .Replace(displayName, " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append(words[0].ToLowerInvariant());

        for (var i = 1; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                {
                    sb.Append(word[1..].ToLowerInvariant());
                }
            }
        }

        return sb.ToString();
    }

    public static bool IsValid(string schemaName) =>
        !string.IsNullOrWhiteSpace(schemaName) &&
        schemaName.Length <= EntityDefinitionLimits.SchemaNameMaxLength &&
        FormatRegex.IsMatch(schemaName) &&
        !IsReserved(schemaName);

    public static bool IsReserved(string schemaName) =>
        EntityDefinitionLimits.ReservedSchemaNames.Contains(schemaName);
}
