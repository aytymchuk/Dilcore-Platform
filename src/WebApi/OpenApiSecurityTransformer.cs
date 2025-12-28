using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.WebApi;

/// <summary>
/// OpenAPI document transformer that adds security schemes for Bearer token and OAuth2 authentication.
/// This enables the Scalar API documentation to display authentication options for Auth0.
/// </summary>
internal sealed class OpenApiSecurityTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var auth0Domain = configuration["Auth0:Domain"];
        var auth0Audience = configuration["Auth0:Audience"];

        // Initialize components if not already present
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // Add Bearer token security scheme for manual token input
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT Bearer token"
        };

        // Add OAuth2 security scheme with Auth0 authorization code flow
        var scopes = new Dictionary<string, string>
        {
            { "openid", "OpenID Connect scope" }
        };

        document.Components.SecuritySchemes["auth0"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Scheme = "OAuth2",
            Description = "Auth0 OAuth2 authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://{auth0Domain}/authorize"),
                    TokenUrl = new Uri($"https://{auth0Domain}/oauth/token"),
                    Scopes = scopes
                }
            }
        };

        // Add security requirements at the document level
        // This makes all endpoints accept either Bearer OR OAuth2 authentication
        var bearerRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };

        var oauth2Requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("auth0", document)] = new List<string> { "openid" }
        };

        // Both schemes are alternatives (either one works)
        document.Security = [bearerRequirement, oauth2Requirement];

        return Task.CompletedTask;
    }
}