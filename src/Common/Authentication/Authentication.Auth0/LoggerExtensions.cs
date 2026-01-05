using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Auth0;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Auth0 UserInfo request successful for user: {UserId}")]
    public static partial void LogAuth0UserInfoSuccess(this ILogger logger, string? userId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Auth0 UserInfo request failed with status code {StatusCode}: {Reason}")]
    public static partial void LogAuth0UserInfoFailed(this ILogger logger, int statusCode, string reason);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Error calling Auth0 UserInfo endpoint")]
    public static partial void LogAuth0UserInfoError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "Auth0 profile cache hit for user: {UserId}")]
    public static partial void LogAuth0ProfileCacheHit(this ILogger logger, string userId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Debug,
        Message = "Auth0 profile cache miss for user: {UserId}")]
    public static partial void LogAuth0ProfileCacheMiss(this ILogger logger, string userId);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Debug,
        Message = "Added claims from Auth0: {Claims}")]
    public static partial void LogClaimsAdded(this ILogger logger, string claims);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Warning,
        Message = "No access token available for Auth0 UserInfo request")]
    public static partial void LogNoAccessToken(this ILogger logger);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Debug,
        Message = "Auth0 profile fetched for user: {UserId}")]
    public static partial void LogAuth0ProfileFetched(this ILogger logger, string userId);
}