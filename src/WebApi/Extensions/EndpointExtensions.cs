using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.WebApi;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Extensions.Api;
using Dilcore.Tenancy.WebApi;
using Dilcore.WebApi.Features.WeatherForecast;
using Dilcore.WebApi.Infrastructure.Validation;
using Dilcore.WebApi.Endpoints;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dilcore.WebApi.Extensions;

public static class EndpointExtensions
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Test endpoint for Problem Details demonstration (Development/Testing only)
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            app.MapTestErrorEndpoint();
            app.MapTestValidationEndpoint();
            app.MapTestTenantEndpoint();
            app.MapTestPublicEndpoint();
            app.MapTestUserInfoEndpoint();
        }

        app.MapWeatherForecastEndpoint();
        app.MapGitHubEndpoint();

        // Domain Module Endpoints
        app.MapIdentityEndpoints();
        app.MapTenancyEndpoints();

        return app;
    }

    private static void MapGitHubEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/github")
            .WithTags("GitHub");

        group.MapGitHubEndpoints();
    }
    // ... rest of the file (Validation, Tenancy endpoints)


    private static void MapTestErrorEndpoint(this WebApplication app)
    {
        app.MapGet("/test/error/{type}", (string type) =>
        {
            throw type switch
            {
                "notfound" => new KeyNotFoundException("The requested resource was not found."),
                "validation" => new ArgumentException("Invalid input provided."),
                "unauthorized" => new UnauthorizedAccessException("Access denied."),
                "conflict" => new InvalidOperationException("Operation conflict detected."),
                "timeout" => new TimeoutException("The operation timed out."),
                _ => new Exception("An unexpected error occurred.")
            };
        })
        .WithName("TestError")
        .WithTags("Testing")
        .ExcludeFromMultiTenantResolution()
        .AllowAnonymous();
    }

    private static void MapWeatherForecastEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast");

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetWeatherForecastQuery(), cancellationToken);
            return result.ToMinimalApiResult();
        })
        .WithName("GetWeatherForecast");

        group.MapPost("/", async (IMediator mediator, CreateWeatherForecastCommand command, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.ToMinimalApiResult();
        })
        .WithName("CreateWeatherForecast");
    }

    private static void MapTestValidationEndpoint(this WebApplication app)
    {
        app.MapPost("/test/validation", (ValidationDto dto) =>
        {
            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                message = "Validation passed successfully!",
                data = dto
            });
        })
        .WithName("TestValidation")
        .WithTags("Testing")
        .WithDescription("Test endpoint to validate FluentValidation integration")
        .AddValidationFilter<ValidationDto>()
        .ExcludeFromMultiTenantResolution()
        .AllowAnonymous();
    }

    private static void MapTestTenantEndpoint(this WebApplication app)
    {
        app.MapGet("/test/tenant-info", (ITenantContextResolver tenantContextResolver) =>
        {
            var tenantContext = tenantContextResolver.Resolve();

            if (tenantContext.Name is null)
            {
                return Microsoft.AspNetCore.Http.Results.Problem(
                    title: "Tenant Not Found",
                    detail: "No tenant could be resolved from the request. Please provide a valid x-tenant header.",
                    statusCode: 400);
            }

            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                tenantName = tenantContext.Name,
                storageIdentifier = tenantContext.StorageIdentifier
            });
        })
        .WithName("TestTenantInfo")
        .WithTags("Testing")
        .WithDescription("Test endpoint that requires multi-tenancy. Returns tenant information.")
        .AllowAnonymous();
    }

    private static void MapTestPublicEndpoint(this WebApplication app)
    {
        app.MapGet("/test/public-info", () =>
        {
            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                message = "This is a public endpoint that does not require tenant context",
                timestamp = DateTime.UtcNow
            });
        })
        .WithName("TestPublicInfo")
        .WithTags("Testing")
        .WithDescription("Test endpoint that does NOT require multi-tenancy.")
        .ExcludeFromMultiTenantResolution()
        .AllowAnonymous();
    }

    private static void MapTestUserInfoEndpoint(this WebApplication app)
    {
        app.MapGet("/test/user-info", ([FromServices] IUserContextResolver userContextResolver) =>
        {
            var userContext = userContextResolver.Resolve();

            if (userContext.Id == UserContext.Empty.Id)
            {
                return Microsoft.AspNetCore.Http.Results.Problem(
                    title: "User Not Found",
                    detail: "No user could be resolved from the request. Please provide valid authentication.",
                    statusCode: 401);
            }

            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                userId = userContext.Id,
                email = userContext.Email,
                fullName = userContext.FullName
            });
        })
        .WithName("TestUserInfo")
        .WithTags("Testing")
        .WithDescription("Test endpoint that requires authentication. Returns current user information.")
        .ExcludeFromMultiTenantResolution();
    }
}