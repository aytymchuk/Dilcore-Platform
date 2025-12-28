using Dilcore.WebApi;
using Dilcore.WebApi.Extensions;
using Auth0.AspNetCore.Authentication.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

// Add services to the container.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [];
        return Task.CompletedTask;
    });
    // Add security schemes for authentication (Bearer + OAuth2)
    options.AddDocumentTransformer<OpenApiSecurityTransformer>();
});
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
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.JwtBearerOptions = new JwtBearerOptions
    {
        Audience = builder.Configuration["Auth0:Audience"]
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
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference("/api-doc", options =>
    {
        options
            // Prefer auth0 (OAuth2) by default - Bearer is still available but not pre-selected
            .AddPreferredSecuritySchemes("auth0")
            // Configure OAuth2 authorization code flow for Auth0
            .AddAuthorizationCodeFlow("auth0", flow =>
            {
                flow.ClientId = builder.Configuration["Auth0:ClientId"];
                flow.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
                // For Auth0, 'openid' is required for ID tokens
                // The 'audience' parameter grants access to the API
                flow.SelectedScopes = ["openid"];
                // Auth0 requires the audience parameter for token requests
                flow.AddQueryParameter("audience", builder.Configuration["Auth0:Audience"] ?? string.Empty);
            })
            // Configure Bearer token authentication for manual token input
            .AddHttpAuthentication("Bearer", auth =>
            {
                auth.Token = string.Empty; // Users will input their own JWT token
            });
    }).AllowAnonymous();
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
