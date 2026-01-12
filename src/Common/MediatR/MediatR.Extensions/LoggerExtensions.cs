using Microsoft.Extensions.Logging;

namespace Dilcore.MediatR.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Handling {RequestName}")]
    public static partial void LogHandlingRequest(this ILogger logger, string requestName);

    [LoggerMessage(LogLevel.Information, "Handled {RequestName}")]
    public static partial void LogHandledRequest(this ILogger logger, string requestName);

    [LoggerMessage(LogLevel.Warning, "Handled {RequestName} with errors: {Errors}")]
    public static partial void LogRequestFailed(this ILogger logger, string requestName, string errors);

    [LoggerMessage(LogLevel.Error, "Handled {RequestName} failed with exception: {ErrorMessage}")]
    public static partial void LogRequestFailedWithException(this ILogger logger, Exception ex, string requestName, string errorMessage);

    [LoggerMessage(LogLevel.Error, "Error handling {RequestName}")]
    public static partial void LogErrorHandlingRequest(this ILogger logger, Exception ex, string requestName);
}