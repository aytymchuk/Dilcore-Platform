namespace Dilcore.CorrelationId.Abstractions;

/// <summary>
/// Provider for resolving correlation ID context from different sources.
/// </summary>
public interface ICorrelationIdContextProvider
{
    /// <summary>
    /// Priority of this provider. Higher values = higher priority (checked first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to resolve correlation ID context.
    /// Returns null if this provider cannot resolve the correlation ID.
    /// </summary>
    ICorrelationIdContext? GetCorrelationIdContext();
}
