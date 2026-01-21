using Microsoft.Extensions.Logging;

namespace Dilcore.MultiTenant.Orleans.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Tenant context extracted for grain '{GrainType}.{MethodName}': TenantName={TenantName}, StorageIdentifier={StorageIdentifier}")]
    public static partial void LogTenantContextExtracted(
        this ILogger logger,
        string grainType,
        string methodName,
        string? tenantName,
        string? storageIdentifier);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "No tenant context found for incoming grain call '{GrainType}.{MethodName}'")]
    public static partial void LogTenantContextNotFound(
        this ILogger logger,
        string grainType,
        string methodName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Tenant context propagated for outgoing grain call '{GrainType}.{MethodName}': TenantName={TenantName}, StorageIdentifier={StorageIdentifier}")]
    public static partial void LogTenantContextPropagated(
        this ILogger logger,
        string grainType,
        string methodName,
        string? tenantName,
        string? storageIdentifier);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "No tenant context to propagate for outgoing grain call '{GrainType}.{MethodName}'")]
    public static partial void LogTenantContextNotPropagated(
        this ILogger logger,
        string grainType,
        string methodName);
}
