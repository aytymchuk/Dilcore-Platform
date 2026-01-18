using Azure.Data.Tables;
using Dilcore.Configuration.Extensions;
using Dilcore.Extensions.OpenApi;
using Dilcore.Extensions.OpenApi.Abstractions;
using Dilcore.MultiTenant.Extensions.OpenApi;
using Dilcore.Authentication.Auth0;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.Configuration.AspNetCore;
using Dilcore.Identity.WebApi;
using Dilcore.MediatR.Extensions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.MultiTenant.Orleans.Extensions;
using Dilcore.Telemetry.Extensions.OpenTelemetry;
using Dilcore.Tenancy.WebApi;
using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.FluentValidation.Extensions.OpenApi;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure;
using Dilcore.WebApi.Infrastructure.Exceptions;

using Dilcore.WebApi.Settings;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

// Add services to the container
builder.Services.AddAppSettings(builder.Configuration);

var appSettings = builder.Configuration.GetRequiredSettings<ApplicationSettings>();
var authSettings = builder.Configuration.GetRequiredSettings<AuthenticationSettings>();

var buildVersion = builder.Configuration[Dilcore.Configuration.AspNetCore.Constants.BuildVersionKey] ?? Dilcore.Configuration.AspNetCore.Constants.DefaultBuildVersion;

builder.Services.AddOpenApiDocumentation(options =>
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

builder.Services.AddTelemetry(builder.Configuration, builder.Environment);
builder.Services.AddProblemDetailsServices();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCorsPolicy();
builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddAuth0ClaimsTransformation(builder.Configuration);
builder.Services.AddFluentValidation(typeof(Dilcore.WebApi.Program).Assembly);
builder.Services.AddMediatRInfrastructure(typeof(Dilcore.WebApi.Program).Assembly);

// Add domain modules (MediatR handlers, validators, etc.)
builder.Services.AddIdentityModule();
builder.Services.AddTenancyModule();

builder.Services.AddSingleton(TimeProvider.System);

// Configure GitHub HttpClient with resilience policies
builder.Services.AddSingleton<IPolicyRegistry<string>, PolicyRegistry>(requestServices =>
{
    var registry = new PolicyRegistry();
    var logger = requestServices.GetRequiredService<ILogger<Program>>();

    registry.Add("GitHubRetry", GetRetryPolicy(logger));
    registry.Add("GitHubCircuitBreaker", GetCircuitBreakerPolicy(logger));

    return registry;
});

// Configure GitHub HttpClient with resilience policies
builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "Dilcore-Platform");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandlerFromRegistry("GitHubRetry")
.AddPolicyHandlerFromRegistry("GitHubCircuitBreaker");

// Configure multi-tenancy with Orleans-backed tenant store
builder.Services.AddMultiTenancy<AppTenantInfo>(mtb =>
{
    mtb.WithStore<OrleansTenantStore>(ServiceLifetime.Scoped);
});

// Configure Orleans Silo
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var grainsSettings = context.Configuration
        .GetSection(nameof(GrainsSettings))
        .Get<GrainsSettings>() ?? new GrainsSettings();

    // Skip Orleans Azure clustering if StorageAccountName is missing
    if (!string.IsNullOrWhiteSpace(grainsSettings.StorageAccountName))
    {
        // Azure Storage clustering with Managed Identity
        siloBuilder.UseAzureStorageClustering(options =>
        {
            var serviceUri = new Uri(
                $"https://{grainsSettings.StorageAccountName}.table.core.windows.net/");

            options.TableServiceClient = new TableServiceClient(
                serviceUri,
                new Azure.Identity.DefaultAzureCredential());
        });
    }
    else
    {
        // Use localhost clustering when Azure clustering is disabled or misconfigured
        siloBuilder.UseLocalhostClustering();
    }

    siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(options =>
    {
        options.ClusterId = grainsSettings.ClusterId;
        options.ServiceId = grainsSettings.ServiceId;
    });

    // Configure networking endpoints
    siloBuilder.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

    // In-memory grain storage
    siloBuilder.AddMemoryGrainStorage("UserStore");
    siloBuilder.AddMemoryGrainStorage("TenantStore");

    // OpenTelemetry activity propagation
    siloBuilder.AddActivityPropagation();

    // Multi-tenancy support for Orleans
    siloBuilder.AddOrleansTenantContext();
});

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
