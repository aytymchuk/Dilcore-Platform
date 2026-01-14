using Dilcore.Extensions.OpenApi.Abstractions;
using Scalar.AspNetCore;

namespace Dilcore.Extensions.Scalar;

public sealed class ScalarSettings
{
    public string Title { get; set; } = "API Documentation";
    public string Version { get; set; } = "v1";
    public ScalarTheme Theme { get; set; } = ScalarTheme.DeepSpace;
    public string Endpoint { get; set; } = ScalarConstants.Endpoint;
    public ScalarAuthenticationSettings? Authentication { get; set; }
}

public sealed class ScalarAuthenticationSettings
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Audience { get; set; }
    public HashSet<string> Scopes { get; set; } = [];
    public string PreferredSecurityScheme { get; set; } = Constants.Security.Auth0SchemeName;
}
