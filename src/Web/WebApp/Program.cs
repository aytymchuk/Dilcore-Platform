using Dilcore.Configuration.AspNetCore;
using Dilcore.WebApp;
using Dilcore.WebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppConfiguration();

builder.Services.AddWebAppServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Application lifecycle logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogApplicationStarted());
lifetime.ApplicationStopping.Register(() => logger.LogApplicationStopping());
lifetime.ApplicationStopped.Register(() => logger.LogApplicationStopped());

logger.LogStartingApplication();

app.ConfigureWebApp();

await app.RunAsync();
