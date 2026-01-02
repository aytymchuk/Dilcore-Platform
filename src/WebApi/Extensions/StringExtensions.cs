namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Extension methods for string manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>
    /// The string in camelCase (e.g., "HelloWorld" -> "helloWorld").
    /// Returns the original string if it is null or empty.
    /// </returns>
    /// <remarks>
    /// This method is useful for mapping property names to JSON property names or other camelCase identifiers.
    /// It converts the first character to lower case using <see cref="char.ToLowerInvariant(char)"/> and leaves the rest of the string unchanged.
    /// </remarks>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return str.ToLowerInvariant();

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// Converts the string to PascalCase.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>
    /// The string in PascalCase (e.g., "helloWorld" -> "HelloWorld").
    /// Returns the original string if it is null or empty.
    /// </returns>
    /// <remarks>
    /// This method is useful for ensuring standard C# property naming conventions.
    /// It converts the first character to upper case using <see cref="char.ToUpperInvariant(char)"/> and leaves the rest of the string unchanged.
    /// </remarks>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return str.ToUpperInvariant();

        return char.ToUpperInvariant(str[0]) + str[1..];
    }
}
