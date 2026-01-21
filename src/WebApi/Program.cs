using Dilcore.Configuration.AspNetCore;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure;
using Dilcore.WebApi.Infrastructure.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

// Add services to the container
builder.Services
    .AddApiInfrastructure(builder.Configuration)
    .AddAuthenticationServices(builder.Configuration)
    .AddMultiTenancyServices()
    .AddObservability(builder.Configuration, builder.Environment);
builder.Services.AddCorrelationIdTracking();


/*
 * options.ConfigureOptions = apiOptions =>
   {
       apiOptions.AddMultiTenantSupport();
       apiOptions.AddCorrelationIdSupport();
       apiOptions.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
   };
 */

// Add domain modules (MediatR handlers, validators, etc.)
builder.AddDomainModules();

// Configure Orleans Silo
builder.Host.AddOrleansConfiguration();

// Multi-tenancy support for Orleans
siloBuilder.AddOrleansTenantContext();

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
