using System.Collections.ObjectModel;
using System.Net;
using Dilcore.Results.Abstractions;

namespace Dilcore.WebApi.Client.Errors;

/// <summary>
/// Represents an API error with ProblemDetails information.
/// </summary>
public class ApiError : AppError
{
    /// <summary>Gets the HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Gets the request instance identifier.</summary>
    public string? Instance { get; }

    /// <summary>Gets the trace identifier for diagnostics.</summary>
    public string? TraceId { get; }

    /// <summary>Gets the timestamp when the error occurred.</summary>
    public DateTimeOffset? Timestamp { get; }

    /// <summary>Gets additional extension data.</summary>
    public IReadOnlyDictionary<string, object>? Extensions { get; }

    /// <summary>
    /// Returns true when StatusCode is 404 - NotFound
    /// </summary>
    public bool NotFound => StatusCode == (int)HttpStatusCode.NotFound;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="type">The error type.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="instance">The request instance identifier.</param>
    /// <param name="traceId">The trace identifier.</param>
    /// <param name="timestamp">The error timestamp.</param>
    /// <param name="extensions">Additional extension data.</param>
    public ApiError(
        string message,
        string code,
        ErrorType type,
        int statusCode,
        string? instance = null,
        string? traceId = null,
        DateTimeOffset? timestamp = null,
        IDictionary<string, object>? extensions = null)
        : base(message, code, type)
    {
        StatusCode = statusCode;
        Instance = instance;
        TraceId = traceId;
        Timestamp = timestamp;
        Extensions = extensions != null
            ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(extensions))
            : null;
    }

    /// <summary>
    /// Creates an ApiError from HTTP status code and optional ProblemDetails metadata.
    /// </summary>
    public static ApiError FromStatusCode(
        int statusCode,
        string? title = null,
        string? detail = null,
        string? instance = null,
        string? errorCode = null,
        string? traceId = null,
        DateTimeOffset? timestamp = null,
        IDictionary<string, object>? extensions = null)
    {
        var (message, code, errorType) = MapStatusCode(statusCode, title, detail, errorCode);

        return new ApiError(
            message,
            code,
            errorType,
            statusCode,
            instance,
            traceId,
            timestamp,
            extensions);
    }

    private static (string Message, string Code, ErrorType Type) MapStatusCode(
        int statusCode,
        string? title,
        string? detail,
        string? errorCode)
    {
        var message = detail ?? title ?? GetDefaultMessage(statusCode);
        var code = errorCode ?? GetDefaultCode(statusCode);
        var type = GetErrorType(statusCode);

        return (message, code, type);
    }

    private static string GetDefaultMessage(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Validation Failed",
        500 => "Internal Server Error",
        503 => "Service Unavailable",
        _ => $"HTTP {statusCode} Error"
    };

    private static string GetDefaultCode(int statusCode) => statusCode switch
    {
        400 => ProblemDetailsConstants.InvalidRequest,
        401 => ProblemDetailsConstants.Unauthorized,
        403 => ProblemDetailsConstants.Forbidden,
        404 => ProblemDetailsConstants.NotFound,
        409 => ProblemDetailsConstants.Conflict,
        422 => ProblemDetailsConstants.ValidationError,
        500 => ProblemDetailsConstants.UnexpectedError,
        503 => ProblemDetailsConstants.ServiceUnavailable,
        _ => ProblemDetailsConstants.UnexpectedError
    };

    private static ErrorType GetErrorType(int statusCode) => statusCode switch
    {
        400 or 422 => ErrorType.Validation,
        401 => ErrorType.Unauthorized,
        403 => ErrorType.Forbidden,
        404 => ErrorType.NotFound,
        409 => ErrorType.Conflict,
        _ => ErrorType.Unexpected
    };
}