using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Dilcore.WebApi.Infrastructure.OpenApi;

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
builder.Services.AddFluentValidation(typeof(Program).Assembly);

var app = builder.Build();

// Application lifecycle logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogInformation("Application has started and is listening on its configured endpoints."));
lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application is stopping..."));
lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application has been stopped."));

logger.LogInformation("Starting the application...");

// Configure middleware pipeline
app.UseCorsPolicy();
app.UseApplicationMiddleware();

// Map application endpoints
app.MapApplicationEndpoints();

await app.RunAsync();

public partial class Program { }
