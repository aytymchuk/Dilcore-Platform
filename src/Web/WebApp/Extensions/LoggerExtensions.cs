using Microsoft.Extensions.Logging;

namespace Dilcore.WebApp.Extensions;

internal static partial class LoggerExtensions
{
    // Application Lifecycle
    [LoggerMessage(LogLevel.Information, "Starting application")]
    public static partial void LogStartingApplication(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application started")]
    public static partial void LogApplicationStarted(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application stopping")]
    public static partial void LogApplicationStopping(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application stopped")]
    public static partial void LogApplicationStopped(this ILogger logger);
}
