using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.FluentValidation.Extensions.MinimalApi;

/// <summary>
/// Extension methods for configuring FluentValidation services.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Registers all FluentValidation validators from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for validators.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFluentValidation(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }

    /// <summary>
    /// Adds FluentValidation filter to the endpoint route.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder AddValidationFilter<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationEndpointFilter<T>>();
    }
}