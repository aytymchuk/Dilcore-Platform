using Microsoft.Extensions.Logging;

namespace Dilcore.Configuration.AspNetCore;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Debug, "Optional configuration not found: {Key}")]
    public static partial void LogOptionalConfigurationNotFound(this ILogger logger, Exception ex, string key);
}