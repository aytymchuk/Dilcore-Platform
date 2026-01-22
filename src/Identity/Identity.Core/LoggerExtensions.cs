using Microsoft.Extensions.Logging;

namespace Dilcore.Identity.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "User with email '{Email}' already exists")]
    public static partial void LogUserAlreadyExists(this ILogger logger, string email);
}
