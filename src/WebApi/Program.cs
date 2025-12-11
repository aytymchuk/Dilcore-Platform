using System.Security.Claims;
using Scalar.AspNetCore;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddTelemetry(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(endpointPrefix: "/api-doc");
}

app.UseHttpsRedirection();

// Simulate Authentication Middleware
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // Simulate an authenticated user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user-123"),
            new Claim("tenant_id", "tenant-abc")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        
        // Simulate passing Tenant ID via header if not present
        if (!context.Request.Headers.ContainsKey("X-Tenant-ID"))
        {
            context.Request.Headers.Append("X-Tenant-ID", "tenant-abc");
        }

        await next();
    });
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    logger.LogGettingWeatherForecast(5);

    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
