namespace Dilcore.WebApi;

/// <summary>
/// Constants for WebApi-specific functionality.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Scalar API documentation constants.
    /// </summary>
    public static class Scalar
    {
        public const string Endpoint = "/api-doc";
    }

    /// <summary>
    /// Security and authentication constants for OpenAPI.
    /// </summary>
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

    /// <summary>
    /// Problem Details (RFC 7807) field names.
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