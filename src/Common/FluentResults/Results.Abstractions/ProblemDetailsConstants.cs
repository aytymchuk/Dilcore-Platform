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

    // Parsing/Request errors
    public const string InvalidRequest = "INVALID_REQUEST";
    public const string JsonParseError = "JSON_PARSE_ERROR";
    public const string FormatError = "FORMAT_ERROR";

    // Domain-specific errors
    public const string TenantNotResolved = "TENANT_NOT_RESOLVED";
    public const string UserNotResolved = "USER_NOT_RESOLVED";

    // Data validation specific
    public const string DataValidationFailed = "DATA_VALIDATION_FAILED";
    public const string DataValidationTitle = "Validation Failed";
    public const string DataValidationDetail = "One or more validation errors occurred.";
}