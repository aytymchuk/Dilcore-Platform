using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Extensions.OpenApi.Abstractions;

/// <summary>
/// Builder for configuring OpenApi documentation.
/// </summary>
public interface IDilcoreOpenApiBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}
