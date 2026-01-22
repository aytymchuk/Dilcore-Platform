namespace Dilcore.Identity.Contracts;

/// <summary>
/// Constants used for identity contract validation.
/// </summary>
internal static class ValidationConstants
{
    /// <summary>
    /// Regex pattern for names (letters, spaces, hyphens, apostrophes).
    /// </summary>
    public const string NameRegex = @"^[a-zA-Z\s-']*$";

    /// <summary>
    /// Error message for invalid characters in names.
    /// </summary>
    public const string InvalidCharactersMessage = "{PropertyName} contains invalid characters.";
}
