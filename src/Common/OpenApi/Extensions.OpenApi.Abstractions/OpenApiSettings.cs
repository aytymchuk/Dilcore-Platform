using Microsoft.AspNetCore.OpenApi;

namespace Dilcore.Extensions.OpenApi.Abstractions;

public sealed class OpenApiSettings
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public OpenApiAuthenticationSettings? Authentication { get; set; }

    public Action<OpenApiOptions>? ConfigureOptions { get; set; }
}

public sealed class OpenApiAuthenticationSettings
{
    public string? Domain { get; set; }
}