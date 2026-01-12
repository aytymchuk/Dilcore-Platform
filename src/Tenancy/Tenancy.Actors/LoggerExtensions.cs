using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Actors;

/// <summary>
/// LoggerMessage extension methods for Tenancy.Actors.
/// </summary>
internal static partial class LoggerExtensions
{
    // Grain Lifecycle
    [LoggerMessage(LogLevel.Debug, "TenantGrain activating: {GrainKey}")]
    public static partial void LogTenantGrainActivating(this ILogger logger, string grainKey);

    [LoggerMessage(LogLevel.Debug, "TenantGrain deactivating: {GrainKey}, Reason: {Reason}")]
    public static partial void LogTenantGrainDeactivating(this ILogger logger, string grainKey, string reason);

    // Tenant Operations
    [LoggerMessage(LogLevel.Information, "Tenant created: {TenantName}, DisplayName: {DisplayName}")]
    public static partial void LogTenantCreated(this ILogger logger, string tenantName, string displayName);

    [LoggerMessage(LogLevel.Warning, "Tenant already exists: {TenantName}")]
    public static partial void LogTenantAlreadyExists(this ILogger logger, string tenantName);

    [LoggerMessage(LogLevel.Debug, "Tenant not found: {TenantName}")]
    public static partial void LogTenantNotFound(this ILogger logger, string tenantName);
}
