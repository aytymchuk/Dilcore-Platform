using System.Text;
using System.Text.RegularExpressions;

namespace Dilcore.Blueprints.Contracts.EntityDefinitions;

/// <summary>
/// Normalizes display-name-like strings to camelCase schema names for validation purposes.
/// Logic mirrors <c>SchemaNameGenerator</c> in Blueprints.Domain — keep in sync when that changes.
/// </summary>
internal static class SchemaNameHelper
{
    private static readonly Regex NonAlphanumeric = new(@"[^a-zA-Z0-9]+", RegexOptions.Compiled);

    /// <summary>
    /// Converts a string (e.g. "phone-number", "My Field") to camelCase (e.g. "phoneNumber", "myField").
    /// </summary>
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = NonAlphanumeric.Replace(input, " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(words[0].ToLowerInvariant());

        for (var i = 1; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                    sb.Append(word[1..].ToLowerInvariant());
            }
        }

        return sb.ToString();
    }
}
