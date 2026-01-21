using Dilcore.Authentication.Auth0;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.Configuration.AspNetCore;
using Dilcore.Configuration.Extensions;
using Dilcore.Extensions.OpenApi;
using Dilcore.Extensions.OpenApi.Abstractions;
using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.FluentValidation.Extensions.OpenApi;
using Dilcore.Identity.WebApi;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Extensions.OpenApi;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.Telemetry.Extensions.OpenTelemetry;
using Dilcore.Tenancy.WebApi;
using Dilcore.WebApi.Infrastructure;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Dilcore.WebApi.Settings;
using FluentValidation;

namespace Dilcore.WebApi.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppSettings(configuration);

        var appSettings = configuration.GetRequiredSettings<ApplicationSettings>();
        var authSettings = configuration.GetRequiredSettings<AuthenticationSettings>();
        var buildVersion = configuration[Dilcore.Configuration.AspNetCore.Constants.BuildVersionKey] ?? Dilcore.Configuration.AspNetCore.Constants.DefaultBuildVersion;

        services.AddOpenApiDocumentation(options =>
        {
            options.Name = appSettings.Name;
            options.Version = buildVersion;
            options.Authentication = new OpenApiAuthenticationSettings
            {
                Domain = authSettings.Auth0?.Domain
            };

            options.ConfigureOptions = apiOptions =>
            {
                apiOptions.AddMultiTenantSupport();
                apiOptions.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
            };
        });

        services.AddProblemDetailsServices();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddCorsPolicy();

        services.AddFluentValidation(typeof(Dilcore.WebApi.Program).Assembly);
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTelemetry(configuration, environment);
        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuth0Authentication(configuration);
        services.AddAuth0ClaimsTransformation(configuration);
        return services;
    }

    public static IServiceCollection AddMultiTenancyServices(this IServiceCollection services)
    {
        services.AddMultiTenancy<AppTenantInfo>(mtb =>
        {
            mtb.WithStore<OrleansTenantStore>(ServiceLifetime.Scoped);
        });

        return services;
    }

    public static WebApplicationBuilder AddDomainModules(this WebApplicationBuilder builder)
    {
        builder.AddIdentityModule();
        builder.AddTenancyModule();
        return builder;
    }
}
