using System.Security.Claims;
using System.Runtime.Serialization;
using Dilcore.WebApi.Extensions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenTelemetry;
using OpenTelemetry.Logs;
using Shouldly;

namespace Dilcore.WebApi.Tests;

[TestFixture]
public class LoggingTests
{
    private class InMemoryExporter : BaseExporter<LogRecord>
    {
        public List<LogRecord> ExportedRecords { get; } = new();

        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            foreach (var record in batch)
            {
                ExportedRecords.Add(record);
            }
            return ExportResult.Success;
        }
    }

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

        // 2. Setup User Context (via HttpContext)
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user") };
        var identity = new ClaimsIdentity(claims, "Test");
        httpContext.User = new ClaimsPrincipal(identity);

        // Setup endpoint with empty metadata so IsExcludedFromMultiTenant returns false
        var endpoint = new Endpoint(
            requestDelegate: _ => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(),
            displayName: "TestEndpoint");
        httpContext.SetEndpoint(endpoint);

        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // 3. Create attribute providers
        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);
        var userProvider = new UserAttributeProvider(httpContextAccessorMock.Object);

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

        // User Processor with null HttpContext
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Create attribute providers
        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);
        var userProvider = new UserAttributeProvider(httpContextAccessorMock.Object);

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