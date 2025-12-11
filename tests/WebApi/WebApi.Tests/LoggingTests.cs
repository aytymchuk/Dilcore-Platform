using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Security.Claims;
using Shouldly;
using WebApi.Extensions;

namespace WebApi.Tests;

[TestFixture]
public class LoggingTests
{
    private class TestLogRecord
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; } = string.Empty;
        public IReadOnlyList<KeyValuePair<string, object?>>? Attributes { get; set; }
    }

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
    public void TenantAndUserContextProcessor_ShouldEnrichLogs_WhenContextIsPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-ID"] = "test-tenant";
        var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var processor = new TenantAndUserContextProcessor(httpContextAccessor);

        // Re-writing the test to use a real pipeline.
        var exporter = new InMemoryExporter();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddProcessor(processor);
                options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
            });
        });

        var logger = loggerFactory.CreateLogger("Test");

        // Act
        logger.LogInformation("Test Message");

        // Assert
        // Wait briefly for export if async, but SimpleLogRecordExportProcessor is synchronous usually
        exporter.ExportedRecords.ShouldNotBeEmpty();
        var logRecord = exporter.ExportedRecords.First();
        
        logRecord.Attributes.ShouldNotBeNull();
        var attributes = logRecord.Attributes!.ToDictionary(kv => kv.Key, kv => kv.Value);

        attributes.ShouldContainKey("tenant.id");
        attributes["tenant.id"].ShouldBe("test-tenant");

        attributes.ShouldContainKey("user.id");
        attributes["user.id"].ShouldBe("test-user");
    }

    [Test]
    public void TenantAndUserContextProcessor_ShouldHandleMissingContext()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor { HttpContext = null };
        var processor = new TenantAndUserContextProcessor(httpContextAccessor);

        var exporter = new InMemoryExporter();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddProcessor(processor);
                options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
            });
        });

        var logger = loggerFactory.CreateLogger("Test");

        // Act
        logger.LogInformation("Test Message");

        // Assert
        exporter.ExportedRecords.ShouldNotBeEmpty();
        var logRecord = exporter.ExportedRecords.First();

        // Should not throw and should not add attributes
        if (logRecord.Attributes != null)
        {
            var attributes = logRecord.Attributes.ToDictionary(kv => kv.Key, kv => kv.Value);
            attributes.ShouldNotContainKey("tenant.id");
            attributes.ShouldNotContainKey("user.id");
        }
    }
}
