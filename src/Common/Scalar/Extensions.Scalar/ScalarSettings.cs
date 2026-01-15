using System.Text.Json.Serialization;
using Scalar.AspNetCore;

namespace Dilcore.Extensions.Scalar;

public sealed class ScalarSettings
{
    public string Title { get; set; } = ScalarConstants.DefaultTitle;
    public string Version { get; set; } = ScalarConstants.DefaultVersion;
    public ScalarTheme Theme { get; set; } = ScalarTheme.DeepSpace;
    public string Endpoint { get; set; } = ScalarConstants.Endpoint;
    public ScalarAuthenticationSettings? Authentication { get; set; }
}

public sealed class ScalarAuthenticationSettings
{
    public string? ClientId { get; set; }

    /// <summary>
    /// The OAuth2 client secret. This property is excluded from serialization to prevent accidental logging or exposure.
    /// </summary>
    [JsonIgnore]
    public string? ClientSecret { get; set; }

    public string? Audience { get; set; }
    public string? PreferredSecurityScheme { get; set; }

    public HashSet<string> Scopes { get; set; } = [];
}
