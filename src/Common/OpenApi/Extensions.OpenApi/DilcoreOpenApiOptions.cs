using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.Extensions.OpenApi;

/// <summary>
/// Options for configuring OpenApi documentation.
/// </summary>
public class DilcoreOpenApiOptions
{
    /// <summary>
    /// Gets the settings for OpenApi documentation.
    /// </summary>
    public OpenApiSettings Settings { get; set; } = new();

    /// <summary>
    /// Action to configure the underlying OpenApiOptions.
    /// </summary>
    public Action<OpenApiOptions>? ConfigureOptions { get; set; }

    /// <summary>
    /// Action to configure the generated OpenApiDocument.
    /// </summary>
    public Action<OpenApiDocument>? ConfigureDocument { get; set; }
}
