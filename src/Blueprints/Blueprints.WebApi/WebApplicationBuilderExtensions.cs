using Dilcore.Blueprints.Core;
using Dilcore.Blueprints.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.WebApi;

/// <summary>
/// Service collection extensions for Blueprints.WebApi dependency injection.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds all Blueprints module services including Core and WebApi components.
    /// </summary>
    public static WebApplicationBuilder AddBlueprintsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddBlueprintsApplication();

        // Add Store services
        builder.Services.AddBlueprintsStore(builder.Configuration);

        return builder;
    }
}
