using Microsoft.Extensions.Logging;

namespace Dilcore.CorrelationId.Http.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message = "Correlation ID extracted from header: {CorrelationId}")]
    public static partial void LogCorrelationIdExtracted(this ILogger logger, string correlationId);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Debug,
        Message = "Correlation ID generated: {CorrelationId}")]
    public static partial void LogCorrelationIdGenerated(this ILogger logger, string correlationId);

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Debug,
        Message = "Correlation ID resolved by provider '{ProviderName}': {CorrelationId}")]
    public static partial void LogCorrelationIdResolved(
        this ILogger logger,
        string providerName,
        string correlationId);

    [LoggerMessage(
        EventId = 2104,
        Level = LogLevel.Debug,
        Message = "No correlation ID could be resolved from any registered provider")]
    public static partial void LogNoCorrelationIdResolved(this ILogger logger);
}
