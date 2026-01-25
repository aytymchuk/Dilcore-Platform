using System.Net;
using System.Text.Json;
using Dilcore.Results.Abstractions;
using Refit;

namespace Dilcore.WebApi.Client.Errors;

/// <summary>
/// Helper class for creating ApiError instances from various exception types.
/// </summary>
internal static class ApiErrorHelper
{
    /// <summary>
    /// Parses an ApiException and extracts ProblemDetails information.
    /// </summary>
    public static async Task<ApiError> ParseApiException(ApiException apiException)
    {
        var statusCode = (int)apiException.StatusCode;

        // Try to parse ProblemDetails from response content
        if (!string.IsNullOrEmpty(apiException.Content))
        {
            try
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(
                    apiException.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (problemDetails != null)
                {
                    return ApiError.FromStatusCode(
                        statusCode,
                        problemDetails.Title,
                        problemDetails.Detail,
                        problemDetails.Instance,
                        problemDetails.ErrorCode,
                        problemDetails.TraceId,
                        problemDetails.Timestamp,
                        problemDetails.Extensions);
                }
            }
            catch
            {
                // If parsing fails, fall through to default error
            }
        }

        // Create default error from status code and exception message
        return ApiError.FromStatusCode(statusCode, detail: apiException.Message);
    }

    /// <summary>
    /// Creates an ApiError for network-related errors.
    /// </summary>
    public static ApiError CreateNetworkError(HttpRequestException exception)
    {
        return new ApiError(
            "A network error occurred while communicating with the API.",
            "NETWORK_ERROR",
            ErrorType.Unexpected,
            (int)HttpStatusCode.ServiceUnavailable);
    }

    /// <summary>
    /// Creates an ApiError for timeout errors.
    /// </summary>
    public static ApiError CreateTimeoutError()
    {
        return new ApiError(
            "The API request took too long to complete.",
            ProblemDetailsConstants.Timeout,
            ErrorType.Unexpected,
            (int)HttpStatusCode.RequestTimeout);
    }

    /// <summary>
    /// Creates an ApiError for unexpected errors.
    /// </summary>
    public static ApiError CreateUnexpectedError(Exception exception)
    {
        return new ApiError(
            exception.Message,
            ProblemDetailsConstants.UnexpectedError,
            ErrorType.Unexpected,
            (int)HttpStatusCode.InternalServerError);
    }

    private class ProblemDetailsResponse
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
        public string? TraceId { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime? Timestamp { get; set; }
        [System.Text.Json.Serialization.JsonExtensionData]
        public Dictionary<string, object>? Extensions { get; set; }
    }
}
