using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shouldly;

namespace Dilcore.WebApi.Tests;

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

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ApplicationName).Returns("TestService");

        // Register multi-tenancy (required by TenantContextProcessor)
        services.AddMultiTenancy();

        // Act
        services.AddTelemetry(configuration, envMock.Object);
        using var serviceProvider = services.BuildServiceProvider();

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

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ApplicationName).Returns("TestService");

        // Register multi-tenancy (required by TenantContextProcessor)
        services.AddMultiTenancy();

        // Act
        services.AddTelemetry(configuration, envMock.Object);
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify all separate processors are registered
        var tenantContextProcessor = serviceProvider.GetService<TenantContextProcessor>();
        tenantContextProcessor.ShouldNotBeNull("TenantContextProcessor should be registered");

        var userContextProcessor = serviceProvider.GetService<UserContextProcessor>();
        userContextProcessor.ShouldNotBeNull("UserContextProcessor should be registered");

        var tenantActivityProcessor = serviceProvider.GetService<TenantActivityProcessor>();
        tenantActivityProcessor.ShouldNotBeNull("TenantActivityProcessor should be registered");

        var userActivityProcessor = serviceProvider.GetService<UserActivityProcessor>();
        userActivityProcessor.ShouldNotBeNull("UserActivityProcessor should be registered");
    }
}
