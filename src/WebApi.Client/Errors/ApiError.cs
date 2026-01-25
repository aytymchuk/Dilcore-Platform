using Dilcore.Results.Abstractions;

namespace Dilcore.WebApi.Client.Errors;

/// <summary>
/// Represents an API error with ProblemDetails information.
/// </summary>
public class ApiError : AppError
{
    public int StatusCode { get; }
    public string? Instance { get; }
    public string? TraceId { get; }
    public DateTime? Timestamp { get; }
    public Dictionary<string, object>? Extensions { get; }

    public ApiError(
        string message,
        string code,
        ErrorType type,
        int statusCode,
        string? instance = null,
        string? traceId = null,
        DateTime? timestamp = null,
        Dictionary<string, object>? extensions = null)
        : base(message, code, type)
    {
        StatusCode = statusCode;
        Instance = instance;
        TraceId = traceId;
        Timestamp = timestamp;
        Extensions = extensions;
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
        DateTime? timestamp = null,
        Dictionary<string, object>? extensions = null)
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
        503 => ProblemDetailsConstants.Timeout,
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