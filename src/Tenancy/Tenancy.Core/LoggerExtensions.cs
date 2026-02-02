using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "User {UserId} is not registered. Cannot create tenant.")]
    public static partial void LogUserNotRegisteredForTenantCreation(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Warning, "User ID is missing from context. Cannot create tenant.")]
    public static partial void LogUserIdMissingForTenantCreation(this ILogger logger);
}
