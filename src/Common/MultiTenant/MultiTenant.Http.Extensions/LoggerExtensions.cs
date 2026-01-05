using Microsoft.Extensions.Logging;

namespace Dilcore.MultiTenant.Http.Extensions;

internal static partial class LoggerExtensions
{
    // TenantContextResolver
    [LoggerMessage(LogLevel.Debug, "No tenant resolved by any provider")]
    public static partial void LogNoTenantResolved(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Tenant resolved by {Provider}: {TenantName}")]
    public static partial void LogTenantResolved(this ILogger logger, string provider, string tenantName);
}