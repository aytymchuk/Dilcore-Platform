using Dilcore.Authentication.Auth0;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.MediatR.Extensions;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Dilcore.WebApi.Infrastructure.OpenApi;
using Dilcore.Configuration.AspNetCore;
using Dilcore.Telemetry.Extensions.OpenTelemetry;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

// Add services to the container
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddOpenApiDocumentation(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration, builder.Environment);
builder.Services.AddProblemDetailsServices();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCorsPolicy();
builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddAuth0ClaimsTransformation(builder.Configuration);
builder.Services.AddFluentValidation(typeof(Dilcore.WebApi.Program).Assembly);
builder.Services.AddMediatRInfrastructure(typeof(Dilcore.WebApi.Program).Assembly);
builder.Services.AddSingleton(TimeProvider.System);

// Configure GitHub HttpClient with resilience policies
builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Dilcore-Platform");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler((sp, request) => GetRetryPolicy(sp.GetRequiredService<ILogger<Program>>()))
.AddPolicyHandler((sp, request) => GetCircuitBreakerPolicy(sp.GetRequiredService<ILogger<Program>>()));

builder.Services.AddMultiTenancy();

var app = builder.Build();

// Application lifecycle logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogApplicationStarted());
lifetime.ApplicationStopping.Register(() => logger.LogApplicationStopping());
lifetime.ApplicationStopped.Register(() => logger.LogApplicationStopped());

logger.LogStartingApplication();

// Configure middleware pipeline
app.UseCorsPolicy();
app.UseApplicationMiddleware();

// Map application endpoints
app.MapApplicationEndpoints();

await app.RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                logger.LogRetryWarning(
                    retryAttempt,
                    timespan.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration, context) =>
            {
                logger.LogCircuitBreakerOpened(
                    duration.TotalSeconds,
                    outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
            },
            onReset: context =>
            {
                logger.LogCircuitBreakerReset();
            });
}

namespace Dilcore.WebApi
{
    public partial class Program { }
}
