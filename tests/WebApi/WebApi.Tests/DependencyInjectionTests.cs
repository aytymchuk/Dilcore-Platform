using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Shouldly;
using WebApi.Extensions;

namespace WebApi.Tests;

[TestFixture]
public class DependencyInjectionTests
{
    [Test]
    public void AddTelemetry_ShouldRegisterOpenTelemetryServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Telemetry:ServiceName", "TestService"},
                {"Telemetry:ConnectionString", "InstrumentationKey=00000000-0000-0000-0000-000000000000;"}
            })
            .Build();

        // Act
        services.AddTelemetry(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify LoggerProvider is registered
        var loggerProvider = serviceProvider.GetService<LoggerProvider>();
        loggerProvider.ShouldNotBeNull("LoggerProvider should be registered");

        // Verify TracerProvider is registered
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull("TracerProvider should be registered");

        // Verify MeterProvider is registered
        var meterProvider = serviceProvider.GetService<MeterProvider>();
        meterProvider.ShouldNotBeNull("MeterProvider should be registered");
    }

    [Test]
    public void AddTelemetry_ShouldRegisterProcessors()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Telemetry:ServiceName", "TestService"}
            })
            .Build();

        // Act
        services.AddTelemetry(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var contextProcessor = serviceProvider.GetService<TenantAndUserContextProcessor>();
        contextProcessor.ShouldNotBeNull("TenantAndUserContextProcessor should be registered");

        var activityProcessor = serviceProvider.GetService<TenantAndUserActivityProcessor>();
        activityProcessor.ShouldNotBeNull("TenantAndUserActivityProcessor should be registered");
    }
}
