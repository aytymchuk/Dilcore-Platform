namespace Dilcore.WebApi;

public static class Constants
{
    public static class Configuration
    {
        public const string SharedKey = "Shared";
        public const string AppConfigEndpointKey = "AppConfigEndpoint";
        public const string BuildVersionKey = "BUILD_VERSION";
        public const string DefaultBuildVersion = "local_development";
    }

    public static class Scalar
    {
        public const string Endpoint = "/api-doc";
    }

    public static class Security
    {
        public const string Auth0SchemeName = "auth0";
        public const string BearerSchemeName = "Bearer";

        public const string BearerHttpScheme = "bearer";
        public const string OAuth2Scheme = "OAuth2";
        public const string BearerFormat = "JWT";

        public const string OpenIdScope = "openid";
        public const string OpenIdScopeDescription = "OpenID Connect scope";

        public const string AudienceParameter = "audience";

        public const string BearerDescription = "Enter your JWT Bearer token";
        public const string Auth0Description = "Auth0 OAuth2 authentication";
    }

    public static class Headers
    {
        public const string Tenant = "x-tenant";
    }

    public static class ProblemDetails
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

        // Data validation specific
        public const string DataValidationFailed = "DATA_VALIDATION_FAILED";
        public const string DataValidationTitle = "Validation Failed";
        public const string DataValidationDetail = "One or more validation errors occurred.";

        // Parsing/Request errors
        public const string InvalidRequest = "INVALID_REQUEST";
        public const string JsonParseError = "JSON_PARSE_ERROR";
        public const string FormatError = "FORMAT_ERROR";
    }
}