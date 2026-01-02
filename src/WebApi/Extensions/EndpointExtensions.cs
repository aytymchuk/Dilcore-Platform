using Dilcore.WebApi.Infrastructure.Validation;

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
        .AllowAnonymous();
    }
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}