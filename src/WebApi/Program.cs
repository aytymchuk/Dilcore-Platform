using Dilcore.Authentication.Auth0;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.OpenTelemetry.Extensions;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Dilcore.WebApi.Infrastructure.OpenApi;
using Dilcore.Configuration.AspNetCore;

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

namespace Dilcore.WebApi
{
    public partial class Program { }
}
