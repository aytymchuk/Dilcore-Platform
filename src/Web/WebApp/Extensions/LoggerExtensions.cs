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

    // Tenant Features
    [LoggerMessage(EventId = 3000, Level = LogLevel.Error, Message = "Failed to create tenant")]
    public static partial void LogTenantCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Error, Message = "Failed to load tenants list: {Error}")]
    public static partial void LogLoadTenantsFailure(this ILogger logger, string error);
}
