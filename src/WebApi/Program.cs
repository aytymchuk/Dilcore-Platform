
using Dilcore.WebApi;
using Dilcore.WebApi.Infrastructure;
using Dilcore.WebApi.Infrastructure.OpenApi;
using Dilcore.WebApi.Infrastructure.Scalar;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Settings;
using Auth0.AspNetCore.Authentication.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

// Add services to the container.
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddOpenApiDocumentation(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration, builder.Environment);
builder.Services.AddCors();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
});

// Add Auth0 Authentication via Auth0.AspNetCore.Authentication.Api
builder.Services.AddAuth0ApiAuthentication(options =>
    {
        var authSettings = builder.Configuration.GetSettings<AuthenticationSettings>();
        options.Domain = authSettings.Auth0?.Domain ?? string.Empty;
        options.JwtBearerOptions = new JwtBearerOptions
        {
            Audience = authSettings.Auth0?.Audience ?? string.Empty
        };
    });

// Enforce authentication for all endpoints by default
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogInformation("Application has started and is listening on its configured endpoints."));
lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application is stopping..."));
lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application has been stopped."));

logger.LogInformation("Starting the application...");

app.UseForwardedHeaders();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation();
    app.AddScalarDocumentation(app.Configuration);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    logger.LogGettingWeatherForecast(5);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

namespace Dilcore.WebApi
{
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}

public partial class Program { }
