namespace Dilcore.WebApi.Settings;

public class AuthenticationSettings
{
    public Auth0Settings? Auth0 { get; set; }
}

public class Auth0Settings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}