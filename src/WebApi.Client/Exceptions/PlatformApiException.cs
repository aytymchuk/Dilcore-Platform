using Refit;

namespace Dilcore.WebApi.Client.Exceptions;

/// <summary>
/// Exception thrown when a Platform API request fails.
/// Wraps Refit's ApiException for cleaner API surface.
/// </summary>
public class PlatformApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="content">The response content.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlatformApiException(
        string message,
        System.Net.HttpStatusCode statusCode,
        string? content = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Content = content;
    }

    /// <summary>
    /// Gets the HTTP status code from the failed request.
    /// </summary>
    public System.Net.HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the response content from the failed request.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Creates a PlatformApiException from a Refit ApiException.
    /// </summary>
    /// <param name="apiException">The Refit API exception.</param>
    /// <returns>A new PlatformApiException instance.</returns>
    public static PlatformApiException FromApiException(ApiException apiException)
    {
        var message = $"API request failed with status {apiException.StatusCode}: {apiException.Message}";
        return new PlatformApiException(
            message,
            apiException.StatusCode,
            apiException.Content,
            apiException);
    }
}
