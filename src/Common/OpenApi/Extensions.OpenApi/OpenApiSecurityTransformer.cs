using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.Extensions.OpenApi;

/// <summary>
/// OpenAPI document transformer that adds security schemes for Bearer token and OAuth2 authentication.
/// This enables the Scalar API documentation to display authentication options for Auth0.
/// </summary>
internal sealed class OpenApiSecurityTransformer(OpenApiSettings settings) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSettings = settings.Authentication;
        if (string.IsNullOrWhiteSpace(authenticationSettings?.Domain))
        {
            return Task.CompletedTask;
        }

        InitializeComponents(document);
        AddBearerSecurityScheme(document);
        AddAuth0SecurityScheme(document, authenticationSettings.Domain);
        AddSecurityRequirements(document);

        return Task.CompletedTask;
    }

    private static void InitializeComponents(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    }

    private static void AddBearerSecurityScheme(OpenApiDocument document)
    {
        document.Components!.SecuritySchemes![Constants.Security.BearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = Constants.Security.BearerHttpScheme,
            BearerFormat = Constants.Security.BearerFormat,
            Description = Constants.Security.BearerDescription
        };
    }

    private static void AddAuth0SecurityScheme(OpenApiDocument document, string auth0Domain)
    {
        var scopes = new Dictionary<string, string>
        {
            { Constants.Security.OpenIdScope, Constants.Security.OpenIdScopeDescription }
        };

        document.Components!.SecuritySchemes![Constants.Security.Auth0SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Scheme = Constants.Security.OAuth2Scheme,
            Description = Constants.Security.Auth0Description,
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
    }

    private static void AddSecurityRequirements(OpenApiDocument document)
    {
        var bearerRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Constants.Security.BearerSchemeName, document)] = new List<string>()
        };

        var oauth2Requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Constants.Security.Auth0SchemeName, document)] = new List<string> { Constants.Security.OpenIdScope }
        };

        document.Security = [bearerRequirement, oauth2Requirement];
    }
}