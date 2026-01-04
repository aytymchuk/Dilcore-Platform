
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApi.Infrastructure.Validation;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

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
        }

        app.MapWeatherForecastEndpoint();

        return app;
    }

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
        app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
        {
            logger.LogGettingWeatherForecast(5);

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");
    }

    private static void MapTestValidationEndpoint(this WebApplication app)
    {
        app.MapPost("/test/validation", (ValidationDto dto) =>
        {
            return Results.Ok(new
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
        app.MapGet("/test/tenant-info", (ITenantContext tenantContext) =>
        {
            if (tenantContext.Name is null)
            {
                return Results.Problem(
                    title: "Tenant Not Found",
                    detail: "No tenant could be resolved from the request. Please provide a valid x-tenant header.",
                    statusCode: 400);
            }

            return Results.Ok(new
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
            return Results.Ok(new
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
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}