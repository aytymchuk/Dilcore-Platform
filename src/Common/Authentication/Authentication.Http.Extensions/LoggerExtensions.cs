using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Http.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "User resolved by provider '{ProviderName}': UserId={UserId}")]
    public static partial void LogUserResolved(this ILogger logger, string providerName, string? userId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "No user could be resolved from any registered provider")]
    public static partial void LogNoUserResolved(this ILogger logger);
}