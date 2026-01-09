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
}
