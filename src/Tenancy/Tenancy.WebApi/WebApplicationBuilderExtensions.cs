using Dilcore.Tenancy.Contracts.Tenants.Create;
using Dilcore.Tenancy.Core;
using Dilcore.Tenancy.Store;
using FluentValidation;
using Microsoft.AspNetCore.Builder;

namespace Dilcore.Tenancy.WebApi;

/// <summary>
/// Service collection extensions for Tenancy.WebApi dependency injection.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds all Tenancy module services including Core and WebApi components.
    /// </summary>
    public static WebApplicationBuilder AddTenancyModule(this WebApplicationBuilder builder)
    {
        // Add Tenancy Store services
        builder.Services.AddTenancyStore(builder.Configuration);

        // Add Tenancy Core services (MediatR handlers and behaviors)
        builder.Services.AddTenancyApplication();

        // Register validators from Contracts assembly for API validation
        builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantDtoValidator).Assembly);

        return builder;
    }
}
