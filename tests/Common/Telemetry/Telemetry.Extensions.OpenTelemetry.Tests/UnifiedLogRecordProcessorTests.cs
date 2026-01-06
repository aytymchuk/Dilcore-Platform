using System.Runtime.Serialization;
using Dilcore.Authentication.Abstractions;
using Dilcore.Authentication.Http.Extensions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenTelemetry.Logs;
using Shouldly;

namespace Dilcore.Telemetry.Extensions.OpenTelemetry.Tests;

[TestFixture]
public class UnifiedLogRecordProcessorTests
{
    [Test]
    public void UnifiedLogRecordProcessor_ShouldEnrichLogs_WhenContextIsPresent()
    {
        // Arrange
        var tenantContext = new TenantContext("test-tenant", "test-shard");
        var userContext = new UserContext("test-user", "test@test.com", "Test User");

        var unifiedProcessor = CreateLogProcessor(tenantContext, userContext);
        var logRecord = CreateLogRecord();

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
        var unifiedProcessor = CreateLogProcessor(null, null);
        var logRecord = CreateLogRecord();

        // Act
        unifiedProcessor.OnEnd(logRecord);

        // Assert
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == TenantConstants.TelemetryTagName);
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == "user.id");
    }

    private UnifiedLogRecordProcessor CreateLogProcessor(ITenantContext? tenantContext, IUserContext? userContext)
    {
        // Tenant setup
        var tenantResolverMock = new Mock<ITenantContextResolver>();
        if (tenantContext != null)
        {
            tenantResolverMock.Setup(x => x.Resolve()).Returns(tenantContext);
            ITenantContext? outContext = tenantContext;
            tenantResolverMock.Setup(x => x.TryResolve(out outContext)).Returns(true);
        }
        else
        {
            tenantResolverMock.Setup(x => x.Resolve()).Returns(TenantContext.Empty);
            ITenantContext? outContext = null;
            tenantResolverMock.Setup(x => x.TryResolve(out outContext)).Returns(false);
        }

        // User setup
        var userResolverMock = new Mock<IUserContextResolver>();
        if (userContext != null)
        {
            IUserContext? outUserContext = userContext;
            userResolverMock.Setup(x => x.TryResolve(out outUserContext)).Returns(true);
        }
        else
        {
            IUserContext? outUserContext = null;
            userResolverMock.Setup(x => x.TryResolve(out outUserContext)).Returns(false);
        }

        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);
        var userProvider = new UserAttributeProvider(userResolverMock.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(tenantProvider);
        serviceCollection.AddSingleton<ITelemetryAttributeProvider>(userProvider);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new UnifiedLogRecordProcessor(serviceProvider, Mock.Of<ILogger<UnifiedLogRecordProcessor>>());
    }

    private LogRecord CreateLogRecord()
    {
#pragma warning disable SYSLIB0050
        var logRecord = (LogRecord)FormatterServices.GetUninitializedObject(typeof(LogRecord));
#pragma warning restore SYSLIB0050
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();
        return logRecord;
    }
}
