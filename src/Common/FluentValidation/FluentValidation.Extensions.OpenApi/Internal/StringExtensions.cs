using System.Diagnostics.CodeAnalysis;

namespace Dilcore.FluentValidation.Extensions.OpenApi.Internal;

/// <summary>
/// Internal extension methods for string manipulation.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return str.ToLowerInvariant();

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return str.ToUpperInvariant();

        return char.ToUpperInvariant(str[0]) + str[1..];
    }
}
