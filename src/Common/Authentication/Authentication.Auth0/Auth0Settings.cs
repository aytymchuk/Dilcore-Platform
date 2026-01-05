namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Configuration settings for Auth0 integration.
/// </summary>
public class Auth0Settings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int UserProfileCacheMinutes { get; set; } = 10;
}
