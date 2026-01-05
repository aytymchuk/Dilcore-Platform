using Dilcore.Authentication.Auth0;

namespace Dilcore.WebApi.Settings;

public class AuthenticationSettings
{
    public Auth0Settings? Auth0 { get; set; }
}