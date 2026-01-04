using System.Diagnostics;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.WebApi.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    [Test]
    public void Verify_DI_Container_Is_Valid()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Telemetry:ServiceName", "TestService"},
                {"Telemetry:ConnectionString", "InstrumentationKey=00000000-0000-0000-0000-000000000000;"},
                {"ApplicationSettings:Name", "TestService"},
                {"AuthenticationSettings:Auth0:Domain", "test-domain"},
                {"AuthenticationSettings:Auth0:Audience", "test-audience"}
            })
            .Build();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");
        envMock.Setup(e => e.ApplicationName).Returns(typeof(Program).Assembly.GetName().Name!);

        // Act
        // Mimic Program.cs registration
        services.AddSingleton<IWebHostEnvironment>(envMock.Object);
        services.AddSingleton<IHostEnvironment>(envMock.Object);
        services.AddControllers();
        services.AddRouting();
        services.AddLogging();
        var diagnosticListener = new DiagnosticListener("Test");
        services.AddSingleton<DiagnosticSource>(diagnosticListener);
        services.AddSingleton(diagnosticListener);

        services.AddAppSettings(configuration);
        services.AddOpenApiDocumentation(configuration);
        services.AddTelemetry(configuration, envMock.Object);
        services.AddProblemDetailsServices();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddCorsPolicy();
        services.AddAuth0Authentication(configuration);
        services.AddFluentValidation(typeof(Program).Assembly);
        services.AddMultiTenancy();

        // Build the service provider with validation options enabled
        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
    }
}
