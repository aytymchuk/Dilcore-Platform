namespace Dilcore.WebApp.Settings;

public class Auth0Settings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
