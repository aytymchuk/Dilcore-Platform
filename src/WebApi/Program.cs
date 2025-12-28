using System.Security.Claims;
using Auth0.AspNetCore.Authentication.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [];
        return Task.CompletedTask;
    });
});
builder.Services.AddTelemetry(builder.Configuration);
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

app.UseForwardedHeaders();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api-doc", options =>
    {
        options.AddOAuth2Authentication("auth0", scheme =>
        {
            scheme.Flows = new ScalarFlows
            {
                AuthorizationCode = new AuthorizationCodeFlow
                {
                    AuthorizationUrl = $"https://{builder.Configuration["Auth0:Domain"]}/authorize",
                    TokenUrl = $"https://{builder.Configuration["Auth0:Domain"]}/oauth/token",
                    ClientId = builder.Configuration["Auth0:ClientId"],
                    ClientSecret = builder.Configuration["Auth0:ClientSecret"]
                }
            };
        });
    });
}

app.UseHttpsRedirection();

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }
