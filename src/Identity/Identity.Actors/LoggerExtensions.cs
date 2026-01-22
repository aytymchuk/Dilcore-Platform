using Microsoft.Extensions.Logging;

namespace Dilcore.Identity.Actors;

/// <summary>
/// LoggerMessage extension methods for Identity.Actors.
/// </summary>
internal static partial class LoggerExtensions
{
    // Grain Lifecycle
    [LoggerMessage(LogLevel.Debug, "UserGrain activating: {GrainKey}")]
    public static partial void LogUserGrainActivating(this ILogger logger, string grainKey);

    [LoggerMessage(LogLevel.Debug, "UserGrain deactivating: {GrainKey}, Reason: {Reason}")]
    public static partial void LogUserGrainDeactivating(this ILogger logger, string grainKey, string reason);

    // User Operations
    [LoggerMessage(LogLevel.Information, "User registered: {UserId}, Email: {Email}")]
    public static partial void LogUserRegistered(this ILogger logger, string userId, string email);

    [LoggerMessage(LogLevel.Warning, "User already registered: {UserId}")]
    public static partial void LogUserAlreadyRegistered(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Debug, "User not found: {UserId}")]
    public static partial void LogUserNotFound(this ILogger logger, string userId);

    // Storage Operations
    [LoggerMessage(LogLevel.Debug, "Reading user state for UserId: {UserId}")]
    public static partial void LogReadingUserState(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Debug, "User state loaded for UserId: {UserId}")]
    public static partial void LogUserStateLoaded(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Debug, "User not found for read: {UserId}")]
    public static partial void LogUserNotFoundForRead(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Error, "Failed to read state for user '{UserId}'")]
    public static partial void LogReadStateError(this ILogger logger, Exception? ex, string userId);

    [LoggerMessage(LogLevel.Debug, "Writing user state for UserId: {UserId}")]
    public static partial void LogWritingUserState(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Debug, "User state written for UserId: {UserId}")]
    public static partial void LogUserStateWritten(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Error, "Failed to write state for user '{UserId}'")]
    public static partial void LogWriteStateError(this ILogger logger, Exception? ex, string userId);

    [LoggerMessage(LogLevel.Debug, "Clearing user state for UserId: {UserId}")]
    public static partial void LogClearingUserState(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Debug, "User state cleared for UserId: {UserId}")]
    public static partial void LogUserStateCleared(this ILogger logger, string userId);

    [LoggerMessage(LogLevel.Error, "Failed to clear state for user '{UserId}'")]
    public static partial void LogClearStateError(this ILogger logger, Exception? ex, string userId);

    [LoggerMessage(LogLevel.Warning, "Invalid user ID for clear operation: {UserId}")]
    public static partial void LogInvalidUserId(this ILogger logger, string userId);
}
