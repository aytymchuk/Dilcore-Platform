using System.Security.Claims;
using System.Runtime.Serialization;
using Dilcore.WebApi.Extensions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
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
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Setup endpoint with empty metadata so IsExcludedFromMultiTenant returns false
        var endpoint = new Endpoint(
            requestDelegate: _ => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(),
            displayName: "TestEndpoint");
        context.SetEndpoint(endpoint);

        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // 3. Create attribute providers
        var tenantProvider = new TenantAttributeProvider(tenantResolverMock.Object);
        var userProvider = new UserAttributeProvider(httpContextAccessorMock.Object);
        var providers = new List<ITelemetryAttributeProvider> { tenantProvider, userProvider };

        // 4. Create unified processor
        var unifiedProcessor = new UnifiedLogRecordProcessor(providers);

        // 5. Setup LogRecord
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
        var providers = new List<ITelemetryAttributeProvider> { tenantProvider, userProvider };

        // Create unified processor
        var unifiedProcessor = new UnifiedLogRecordProcessor(providers);

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