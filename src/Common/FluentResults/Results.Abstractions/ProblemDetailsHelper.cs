namespace Dilcore.Results.Abstractions;

/// <summary>
/// Helper methods for working with Problem Details.
/// </summary>
public static class ProblemDetailsHelper
{
    /// <summary>
    /// Builds a Problem Details type URI from an error code.
    /// </summary>
    /// <param name="errorCode">The error code to include in the URI.</param>
    /// <returns>The full type URI, or the base URI if error code is null/whitespace.</returns>
    public static string BuildTypeUri(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return ProblemDetailsConstants.TypeBaseUri;
        }

        return $"{ProblemDetailsConstants.TypeBaseUri}/{errorCode.ToLowerInvariant().Replace('_', '-')}";
    }
}
