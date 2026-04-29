namespace Dilcore.WebApp.Extensions;

internal static partial class LoggerExtensions
{
    // Application Lifecycle
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Starting application")]
    public static partial void LogStartingApplication(this ILogger logger);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Application started")]
    public static partial void LogApplicationStarted(this ILogger logger);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Application stopping")]
    public static partial void LogApplicationStopping(this ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Application stopped")]
    public static partial void LogApplicationStopped(this ILogger logger);

    [LoggerMessage(EventId = 2000, Level = LogLevel.Error, Message = "An unhandled exception occurred")]
    public static partial void LogUnhandledException(this ILogger logger, Exception ex);

    // Data Operations (MediatR)
    [LoggerMessage(EventId = 2100, Level = LogLevel.Information, Message = "Loading data: {Operation}")]
    public static partial void LogLoadingData(this ILogger logger, string operation);

    [LoggerMessage(EventId = 2101, Level = LogLevel.Information, Message = "Loaded data: {Operation} in {ElapsedMs}ms")]
    public static partial void LogLoadingDataSucceeded(this ILogger logger, string operation, long elapsedMs);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Warning, Message = "Loading data failed: {Operation} in {ElapsedMs}ms. Error: {Error}")]
    public static partial void LogLoadingDataFailed(this ILogger logger, string operation, long elapsedMs, string error);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Error, Message = "Loading data threw an exception: {Operation} in {ElapsedMs}ms")]
    public static partial void LogLoadingDataException(this ILogger logger, Exception ex, string operation, long elapsedMs);

    [LoggerMessage(EventId = 2110, Level = LogLevel.Information, Message = "Saving data: {Operation}")]
    public static partial void LogSavingData(this ILogger logger, string operation);

    [LoggerMessage(EventId = 2111, Level = LogLevel.Information, Message = "Saved data: {Operation} in {ElapsedMs}ms")]
    public static partial void LogSavingDataSucceeded(this ILogger logger, string operation, long elapsedMs);

    [LoggerMessage(EventId = 2112, Level = LogLevel.Warning, Message = "Saving data failed: {Operation} in {ElapsedMs}ms. Error: {Error}")]
    public static partial void LogSavingDataFailed(this ILogger logger, string operation, long elapsedMs, string error);

    [LoggerMessage(EventId = 2113, Level = LogLevel.Error, Message = "Saving data threw an exception: {Operation} in {ElapsedMs}ms")]
    public static partial void LogSavingDataException(this ILogger logger, Exception ex, string operation, long elapsedMs);

    // Component Operations
    [LoggerMessage(EventId = 2200, Level = LogLevel.Information, Message = "Component operation started: {Component} {Operation}. Message: {Message}")]
    public static partial void LogComponentOperationStarted(this ILogger logger, string component, string operation, string? message);

    [LoggerMessage(EventId = 2201, Level = LogLevel.Information, Message = "Component operation succeeded: {Component} {Operation}")]
    public static partial void LogComponentOperationSucceeded(this ILogger logger, string component, string operation);

    [LoggerMessage(EventId = 2202, Level = LogLevel.Error, Message = "Component operation failed: {Component} {Operation}. Message: {Message}")]
    public static partial void LogComponentOperationException(this ILogger logger, Exception ex, string component, string operation, string? message);

    // Tenant Features
    [LoggerMessage(EventId = 3000, Level = LogLevel.Error, Message = "Failed to create tenant")]
    public static partial void LogTenantCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Error, Message = "Failed to load tenants list: {Error}")]
    public static partial void LogLoadTenantsFailure(this ILogger logger, string error);

    // Loading Service
    [LoggerMessage(EventId = 4000, Level = LogLevel.Error, Message = "Error notifying subscriber {SubscriberInfo}")]
    public static partial void LogLoadingSubscriberError(this ILogger logger, Exception ex, string subscriberInfo);
}
