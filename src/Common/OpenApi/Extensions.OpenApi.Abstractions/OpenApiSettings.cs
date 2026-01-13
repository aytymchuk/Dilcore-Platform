namespace Dilcore.Extensions.OpenApi.Abstractions;

public class OpenApiSettings
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public OpenApiAuthenticationSettings? Authentication { get; set; }
}

public class OpenApiAuthenticationSettings
{
    public string? Domain { get; set; }
}
