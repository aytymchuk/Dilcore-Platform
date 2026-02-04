namespace Dilcore.WebApi.Extensions;

internal static partial class LoggerExtensions
{
    // Program.cs - Application lifecycle
    [LoggerMessage(LogLevel.Information, "Application has started and is listening on its configured endpoints.")]
    public static partial void LogApplicationStarted(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application is stopping...")]
    public static partial void LogApplicationStopping(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Application has been stopped.")]
    public static partial void LogApplicationStopped(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Starting the application...")]
    public static partial void LogStartingApplication(this ILogger logger);

    // CorsExtensions
    [LoggerMessage(LogLevel.Warning, "CORS AllowedOrigins is empty in non-development environment. All CORS requests will be blocked.")]
    public static partial void LogCorsAllowedOriginsEmpty(this ILogger logger);

    // AuthenticationExtensions - JWT Bearer Events
    [LoggerMessage(LogLevel.Warning, "Authentication failed for request {Path}: {Message}")]
    public static partial void LogAuthenticationFailed(this ILogger logger, Exception exception, string path, string message);

    [LoggerMessage(LogLevel.Warning, "Authentication challenge issued for request {Path}: {Error} - {ErrorDescription}")]
    public static partial void LogAuthenticationChallenge(this ILogger logger, string path, string error, string errorDescription);

    // UnifiedActivityProcessor & UnifiedLogRecordProcessor
    [LoggerMessage(LogLevel.Error, "Error getting attributes from provider {ProviderType}. Continuing with remaining providers.")]
    public static partial void LogAttributeProviderError(this ILogger logger, Exception ex, string providerType);

    // OrleansTenantStore - Tenant Resolution
    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Querying tenant by identifier '{Identifier}'")]
    public static partial void LogTenantStoreGetActorByIdentifier(this ILogger logger, string identifier);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Tenant '{Identifier}' not found in grain")]
    public static partial void LogTenantStoreNotFound(this ILogger logger, string identifier);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Resolved tenant '{Identifier}' with display name '{DisplayName}'")]
    public static partial void LogTenantStoreResolved(this ILogger logger, string identifier, string displayName);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: GetAllAsync is not supported for Orleans grain store")]
    public static partial void LogTenantStoreGetAllNotSupported(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: {Operation} is not supported - use TenantGrain directly")]
    public static partial void LogTenantStoreModificationNotSupported(this ILogger logger, string operation);

    [LoggerMessage(LogLevel.Warning, "OrleansTenantStore: Invalid identifier provided")]
    public static partial void LogTenantStoreInvalidIdentifier(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "OrleansTenantStore: Error resolving tenant '{Identifier}'")]
    public static partial void LogTenantStoreResolutionError(this ILogger logger, Exception ex, string identifier);

    // UserClaimsTransformation
    [LoggerMessage(LogLevel.Debug, "Added {Count} tenant claims for user {UserId}")]
    public static partial void LogTenantsAdded(this ILogger logger, string userId, int count);

    [LoggerMessage(LogLevel.Debug, "Added {Count} role claims for user {UserId} in tenant {TenantId}")]
    public static partial void LogRolesAdded(this ILogger logger, string userId, string tenantId, int count);

    [LoggerMessage(LogLevel.Error, "Error transforming claims for user {UserId}")]
    public static partial void LogTransformationError(this ILogger logger, Exception ex, string userId);

    // UserTenantAuthorizeMiddleware
    [LoggerMessage(LogLevel.Warning, "Access to tenant {TenantIdentifier} forbidden for user {UserId}")]
    public static partial void LogTenantAccessForbidden(this ILogger logger, string userId, string tenantIdentifier);
}