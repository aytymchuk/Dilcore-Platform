namespace Dilcore.WebApi.Extensions;

internal static partial class LoggerExtensions
{
    // EndpointExtensions
    [LoggerMessage(LogLevel.Information, "Getting weather forecast for {Count} days")]
    public static partial void LogGettingWeatherForecast(this ILogger logger, int count);

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

    // Resilience Policies
    [LoggerMessage(LogLevel.Warning, "Retry {RetryAttempt} after {Delay}s due to {Exception}")]
    public static partial void LogRetryWarning(this ILogger logger, int retryAttempt, double delay, string exception);

    [LoggerMessage(LogLevel.Warning, "Circuit breaker opened for {Duration}s due to {Exception}")]
    public static partial void LogCircuitBreakerOpened(this ILogger logger, double duration, string exception);

    [LoggerMessage(LogLevel.Information, "Circuit breaker reset")]
    public static partial void LogCircuitBreakerReset(this ILogger logger);

    // OrleansTenantStore - Tenant Resolution
    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Querying tenant by identifier '{Identifier}'")]
    public static partial void LogTenantStoreGetByIdentifier(this ILogger logger, string identifier);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Tenant '{Identifier}' not found in grain")]
    public static partial void LogTenantStoreNotFound(this ILogger logger, string identifier);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: Resolved tenant '{Identifier}' with display name '{DisplayName}'")]
    public static partial void LogTenantStoreResolved(this ILogger logger, string identifier, string displayName);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: GetAllAsync is not supported for Orleans grain store")]
    public static partial void LogTenantStoreGetAllNotSupported(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "OrleansTenantStore: {Operation} is not supported - use TenantGrain directly")]
    public static partial void LogTenantStoreModificationNotSupported(this ILogger logger, string operation);
}