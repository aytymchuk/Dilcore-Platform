using System.Runtime.Serialization;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenTelemetry.Logs;
using Shouldly;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.Authentication.Abstractions;

namespace Dilcore.OpenTelemetry.Extensions.Tests;

[TestFixture]
public class UnifiedLogRecordProcessorTests
{
    [Test]
    public void UnifiedLogRecordProcessor_ShouldEnrichLogs_WhenContextIsPresent()
    {
        // Arrange
        // 1. Setup Tenant Context (via Resolver)
        var tenantContext = new TenantContext("test-tenant", "test-shard");
        var tenantResolverMock = new Mock<ITenantContextResolver>();
        tenantResolverMock.Setup(x => x.Resolve()).Returns(tenantContext);

        ITenantContext? outContext = tenantContext;
        tenantResolverMock.Setup(x => x.TryResolve(out outContext)).Returns(true);

        // 2. Setup User Context (via HttpContext and Resolver)
        var userContext = new UserContext("test-user", "test@test.com", "Test User");
        var userResolverMock = new Mock<IUserContextResolver>();
        IUserContext? outUserContext = userContext;
        userResolverMock.Setup(x => x.TryResolve(out outUserContext)).Returns(true);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IUserContextResolver))).Returns(userResolverMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // 3. Create attribute providers
        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);

        // Mock IServiceProvider for UserAttributeProvider to resolve IHttpContextAccessor
        var userProviderServiceProviderMock = new Mock<IServiceProvider>();
        userProviderServiceProviderMock.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(httpContextAccessorMock.Object);
        var userProvider = new UserAttributeProvider(userProviderServiceProviderMock.Object);

        // 4. Create IServiceProvider mock
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(tenantProvider);
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(userProvider);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // 5. Create unified processor
        var loggerMock = new Mock<ILogger<UnifiedLogRecordProcessor>>();
        var unifiedProcessor = new UnifiedLogRecordProcessor(serviceProvider, loggerMock.Object);

        // 6. Setup LogRecord
#pragma warning disable SYSLIB0050
        var logRecord = (LogRecord)FormatterServices.GetUninitializedObject(typeof(LogRecord));
#pragma warning restore SYSLIB0050
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();

        // Act
        unifiedProcessor.OnEnd(logRecord);

        // Assert
        var attributes = logRecord.Attributes.ToDictionary(kv => kv.Key, kv => kv.Value);

        attributes.ShouldContainKey(TenantConstants.TelemetryTagName);
        attributes[TenantConstants.TelemetryTagName].ShouldBe("test-tenant");

        attributes.ShouldContainKey("user.id");
        attributes["user.id"].ShouldBe("test-user");
    }

    [Test]
    public void UnifiedLogRecordProcessor_ShouldHandleMissingContext()
    {
        // Arrange
        var tenantResolverMock = new Mock<ITenantContextResolver>();
        tenantResolverMock.Setup(x => x.Resolve()).Returns(TenantContext.Empty);

        ITenantContext? outContext = null;
        tenantResolverMock.Setup(x => x.TryResolve(out outContext)).Returns(false);

        // User Processor with null context
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Create attribute providers
        // Create service provider for UserAttributeProvider
        var userProviderServiceProviderMock = new Mock<IServiceProvider>();
        userProviderServiceProviderMock.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(httpContextAccessorMock.Object);

        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);
        var userProvider = new UserAttributeProvider(userProviderServiceProviderMock.Object);

        // Create IServiceProvider mock
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(tenantProvider);
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(userProvider);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Create unified processor
        var loggerMock = new Mock<ILogger<UnifiedLogRecordProcessor>>();
        var unifiedProcessor = new UnifiedLogRecordProcessor(serviceProvider, loggerMock.Object);

#pragma warning disable SYSLIB0050
        var logRecord = (LogRecord)FormatterServices.GetUninitializedObject(typeof(LogRecord));
#pragma warning restore SYSLIB0050
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();

        // Act
        unifiedProcessor.OnEnd(logRecord);

        // Assert
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == TenantConstants.TelemetryTagName);
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == "user.id");
    }
}
