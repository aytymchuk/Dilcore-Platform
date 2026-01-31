using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Actors;

/// <summary>
/// LoggerMessage extension methods for Tenancy.Actors.
/// </summary>
public static partial class LoggerExtensions
{
    // Grain Lifecycle
    [LoggerMessage(LogLevel.Debug, "TenantGrain activating: {GrainKey}")]
    public static partial void LogTenantGrainActivating(this ILogger logger, string grainKey);

    [LoggerMessage(LogLevel.Debug, "TenantGrain deactivating: {GrainKey}, Reason: {Reason}")]
    public static partial void LogTenantGrainDeactivating(this ILogger logger, string grainKey, string reason);

    // Tenant Operations
    [LoggerMessage(LogLevel.Information, "Tenant created: {TenantName}, Name: {Name}")]
    public static partial void LogTenantCreated(this ILogger logger, string tenantName, string name);

    [LoggerMessage(LogLevel.Warning, "Tenant already exists: {TenantName}")]
    public static partial void LogTenantAlreadyExists(this ILogger logger, string tenantName);

    [LoggerMessage(LogLevel.Debug, "Tenant not found: {TenantName}")]
    public static partial void LogTenantNotFound(this ILogger logger, string tenantName);

    // Tenant Storage
    [LoggerMessage(LogLevel.Debug, "Reading state for tenant: {SystemName}")]
    public static partial void LogReadingTenantState(this ILogger logger, string systemName);

    [LoggerMessage(LogLevel.Error, "Error reading state for tenant {SystemName}")]
    public static partial void LogReadStateError(this ILogger logger, Exception? ex, string systemName);

    [LoggerMessage(LogLevel.Warning, "Tenant not found for read: {SystemName}")]
    public static partial void LogTenantNotFoundForRead(this ILogger logger, string systemName);

    [LoggerMessage(LogLevel.Debug, "Tenant state loaded: {SystemName}")]
    public static partial void LogTenantStateLoaded(this ILogger logger, string systemName);

    [LoggerMessage(LogLevel.Debug, "Writing state for tenant: {SystemName}")]
    public static partial void LogWritingTenantState(this ILogger logger, string systemName);

    [LoggerMessage(LogLevel.Error, "Error writing state for tenant {SystemName}")]
    public static partial void LogWriteStateError(this ILogger logger, Exception? ex, string systemName);

    [LoggerMessage(LogLevel.Debug, "Tenant state written: {SystemName}")]
    public static partial void LogTenantStateWritten(this ILogger logger, string systemName);

    [LoggerMessage(LogLevel.Debug, "Clearing state for tenant: {SystemName}")]
    public static partial void LogClearingTenantState(this ILogger logger, string systemName);
}
