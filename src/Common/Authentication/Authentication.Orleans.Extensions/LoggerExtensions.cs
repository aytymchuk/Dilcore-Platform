using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Orleans.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "User context extracted for grain '{GrainType}.{MethodName}': UserId={UserId}, Email={Email}")]
    public static partial void LogUserContextExtracted(
        this ILogger logger,
        string grainType,
        string methodName,
        string? userId,
        string? email);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "No user context found for incoming grain call '{GrainType}.{MethodName}'")]
    public static partial void LogUserContextNotFound(
        this ILogger logger,
        string grainType,
        string methodName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "User context propagated for outgoing grain call '{GrainType}.{MethodName}': UserId={UserId}, Email={Email}")]
    public static partial void LogUserContextPropagated(
        this ILogger logger,
        string grainType,
        string methodName,
        string? userId,
        string? email);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "No user context to propagate for outgoing grain call '{GrainType}.{MethodName}'")]
    public static partial void LogUserContextNotPropagated(
        this ILogger logger,
        string grainType,
        string methodName);
}
