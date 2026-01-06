namespace Dilcore.Results.Abstractions;

public static class ProblemDetailsConstants
{
    public const string ContentType = "application/problem+json";
    public const string TypeBaseUri = "https://api.dilcore.com/errors";

    // Standard error codes
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string NotImplemented = "NOT_IMPLEMENTED";
    public const string OperationCancelled = "OPERATION_CANCELLED";
    public const string Timeout = "TIMEOUT";
}