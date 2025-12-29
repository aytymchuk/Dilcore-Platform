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
}
