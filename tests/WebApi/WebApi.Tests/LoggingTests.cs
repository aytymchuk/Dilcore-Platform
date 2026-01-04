using System.Security.Claims;
using System.Runtime.Serialization;
using System.Security.Claims;
using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Http;
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
    public void TenantAndUserContextProcessors_ShouldEnrichLogs_WhenContextIsPresent()
    {
        // Arrange
        // 1. Setup Tenant Context (via Resolver)
        var tenantContext = new TenantContext("test-tenant", "test-shard");
        var tenantResolverMock = new Mock<ITenantContextResolver>();
        tenantResolverMock.Setup(x => x.Resolve()).Returns(tenantContext);

        var tenantProcessor = new TenantContextProcessor(tenantResolverMock.Object);

        // 2. Setup User Context (via HttpContext)
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var userProcessor = new UserContextProcessor(httpContextAccessorMock.Object);

        // 3. Setup LogRecord
        // Note: LogRecord has internal constructor in some versions, but we can usually create it or use reflection/helpers if needed. 
        // If 'new LogRecord()' fails, we might need a workaround or use 'Activity' for traces.
        // However, LogRecord in recent OpenTelemetry versions often allows parameterless init or property setting.
        // Let's rely on how it was done before or standard usage. 
        // Since the previous code used `new LogRecord()`, I will assume it is valid or I will mock the behavior if possible.
        // Actually, easiest way to test Processor logic is creating it manually if allowed.

        var logRecord = (LogRecord)FormatterServices.GetUninitializedObject(typeof(LogRecord));
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();

        // Act
        tenantProcessor.OnEnd(logRecord);
        userProcessor.OnEnd(logRecord);

        // Assert
        var attributes = logRecord.Attributes.ToDictionary(kv => kv.Key, kv => kv.Value);

        attributes.ShouldContainKey("tenant.id");
        attributes["tenant.id"].ShouldBe("test-tenant");

        attributes.ShouldContainKey("user.id");
        attributes["user.id"].ShouldBe("test-user");
    }

    [Test]
    public void TenantAndUserContextProcessors_ShouldHandleMissingContext()
    {
        // Arrange
        var tenantResolverMock = new Mock<ITenantContextResolver>();
        tenantResolverMock.Setup(x => x.Resolve()).Returns((TenantContext?)null);

        var tenantProcessor = new TenantContextProcessor(tenantResolverMock.Object);

        // User Processor with null HttpContext
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var userProcessor = new UserContextProcessor(httpContextAccessorMock.Object);

        var logRecord = (LogRecord)FormatterServices.GetUninitializedObject(typeof(LogRecord));
        logRecord.Attributes = new List<KeyValuePair<string, object?>>();

        // Act
        tenantProcessor.OnEnd(logRecord);
        userProcessor.OnEnd(logRecord);

        // Assert
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == "tenant.id");
        logRecord.Attributes.ShouldNotContain(kv => kv.Key == "user.id");
    }
}