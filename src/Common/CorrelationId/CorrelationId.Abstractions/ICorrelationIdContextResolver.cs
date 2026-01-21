namespace Dilcore.CorrelationId.Abstractions;

/// <summary>
/// Resolves correlation ID context by trying registered providers in priority order.
/// </summary>
public interface ICorrelationIdContextResolver
{
    /// <summary>
    /// Resolves the current correlation ID context.
    /// </summary>
    /// <returns>The resolved correlation ID context.</returns>
    ICorrelationIdContext Resolve();

    /// <summary>
    /// Attempts to resolve the correlation ID context.
    /// </summary>
    /// <param name="correlationIdContext">The resolved correlation ID context, or null if resolution failed.</param>
    /// <returns>True if correlation ID context was successfully resolved; otherwise, false.</returns>
    bool TryResolve(out ICorrelationIdContext? correlationIdContext);
}
