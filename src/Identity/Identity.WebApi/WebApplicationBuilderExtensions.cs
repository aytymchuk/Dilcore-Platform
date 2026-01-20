using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.Identity.Core;
using Dilcore.Identity.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Identity.WebApi;

/// <summary>
/// Service collection extensions for Identity.WebApi dependency injection.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds all Identity module services including Core and WebApi components.
    /// </summary>
    public static WebApplicationBuilder AddIdentityModule(this WebApplicationBuilder builder)
    {
        // Add Identity Core services (MediatR handlers and behaviors)
        builder.Services.AddIdentityApplication();
        builder.Services.AddIdentityStore(builder.Configuration);

        // Register FluentValidation validators from Identity.Contracts
        builder.Services.AddFluentValidation(typeof(Contracts.Register.RegisterUserDto).Assembly);

        return builder;
    }
}
