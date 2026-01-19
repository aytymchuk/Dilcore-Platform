using Dilcore.Tenancy.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Tenancy.WebApi;

/// <summary>
/// Service collection extensions for Tenancy.WebApi dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Tenancy module services including Core and WebApi components.
    /// </summary>
    public static WebApplicationBuilder AddTenancyModule(this WebApplicationBuilder builder)
    {
        // Add Tenancy Core services (MediatR handlers and behaviors)
        builder.Services.AddTenancyApplication();

        return builder;
    }
}
