using Microsoft.Extensions.Logging;

namespace Dilcore.Blueprints.Actors;

public static partial class LoggerExtensions
{
    // Grain Lifecycle
    [LoggerMessage(LogLevel.Debug, "EntityDefinitionGrain activating: {GrainId}")]
    public static partial void LogEntityDefinitionGrainActivating(this ILogger logger, Guid grainId);

    [LoggerMessage(LogLevel.Debug, "EntityDefinitionGrain deactivating: {GrainId}, Reason: {Reason}")]
    public static partial void LogEntityDefinitionGrainDeactivating(this ILogger logger, Guid grainId, string reason);

    // Entity Definition Operations
    [LoggerMessage(LogLevel.Information, "Entity definition created: {GrainId}, SchemaName: {SchemaName}")]
    public static partial void LogEntityDefinitionCreated(this ILogger logger, Guid grainId, string schemaName);

    [LoggerMessage(LogLevel.Information, "Entity definition updated: {GrainId}, SchemaName: {SchemaName}")]
    public static partial void LogEntityDefinitionUpdated(this ILogger logger, Guid grainId, string schemaName);

    [LoggerMessage(LogLevel.Warning, "Entity definition already exists: {GrainId}")]
    public static partial void LogEntityDefinitionAlreadyExists(this ILogger logger, Guid grainId);

    [LoggerMessage(LogLevel.Debug, "Entity definition not found: {GrainId}")]
    public static partial void LogEntityDefinitionNotFound(this ILogger logger, Guid grainId);

    [LoggerMessage(LogLevel.Warning, "ETag mismatch for entity definition {GrainId}: expected {CurrentETag}, got {IncomingETag}")]
    public static partial void LogEntityDefinitionETagMismatch(this ILogger logger, Guid grainId, long incomingETag, long currentETag);

    [LoggerMessage(LogLevel.Information, "Entity definition deleted: {GrainId}, SchemaName: {SchemaName}")]
    public static partial void LogEntityDefinitionDeleted(this ILogger logger, Guid grainId, string schemaName);

    // Blueprint Definition Storage (generic)
    [LoggerMessage(LogLevel.Debug, "Reading {StateType} state: {Id}")]
    public static partial void LogReadingState(this ILogger logger, string stateType, Guid id);

    [LoggerMessage(LogLevel.Error, "Error reading {StateType} state: {Id}")]
    public static partial void LogReadStateError(this ILogger logger, Exception? ex, string stateType, Guid id);

    [LoggerMessage(LogLevel.Warning, "{StateType} not found for read: {Id}")]
    public static partial void LogStateNotFoundForRead(this ILogger logger, string stateType, Guid id);

    [LoggerMessage(LogLevel.Debug, "{StateType} state loaded: {Id}")]
    public static partial void LogStateLoaded(this ILogger logger, string stateType, Guid id);

    [LoggerMessage(LogLevel.Debug, "Writing {StateType} state: {Id}")]
    public static partial void LogWritingState(this ILogger logger, string stateType, Guid id);

    [LoggerMessage(LogLevel.Error, "Error writing {StateType} state: {Id}")]
    public static partial void LogWriteStateError(this ILogger logger, Exception? ex, string stateType, Guid id);

    [LoggerMessage(LogLevel.Debug, "{StateType} state written: {Id}")]
    public static partial void LogStateWritten(this ILogger logger, string stateType, Guid id);

    [LoggerMessage(LogLevel.Debug, "Clearing {StateType} state: {Id}")]
    public static partial void LogClearingState(this ILogger logger, string stateType, Guid id);
}
