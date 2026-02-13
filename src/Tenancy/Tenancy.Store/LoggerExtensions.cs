using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Store;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Error, "Failed to store tenant {TenantId}")]
    public static partial void LogStoreTenantFailed(this ILogger logger, Exception ex, Guid tenantId);

    [LoggerMessage(LogLevel.Error, "Failed to get tenant by system name {SystemName}")]
    public static partial void LogGetTenantBySystemNameFailed(this ILogger logger, Exception ex, string systemName);

    [LoggerMessage(LogLevel.Error, "Failed to get {Count} tenants by system names")]
    public static partial void LogGetTenantsBySystemNamesFailed(this ILogger logger, Exception ex, int count);
}
