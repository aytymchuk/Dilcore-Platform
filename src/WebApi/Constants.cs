namespace Dilcore.WebApi;

/// <summary>
/// Constants for WebApi-specific functionality.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Problem Details (RFC 9457, which obsoletes RFC 7807) field names.
    /// </summary>
    public static class ProblemDetails
    {
        /// <summary>
        /// Problem Details field names.
        /// </summary>
        public static class Fields
        {
            public const string Type = "type";
            public const string Title = "title";
            public const string Status = "status";
            public const string Detail = "detail";
            public const string Instance = "instance";
            public const string TraceId = "traceId";
            public const string ErrorCode = "errorCode";
            public const string Timestamp = "timestamp";
            public const string Errors = "errors";
        }
    }
}