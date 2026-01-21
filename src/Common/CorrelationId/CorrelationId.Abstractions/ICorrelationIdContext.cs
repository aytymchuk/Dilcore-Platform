namespace Dilcore.CorrelationId.Abstractions;

/// <summary>
/// Abstraction for accessing correlation ID across different contexts.
/// Used for distributed tracing and request tracking.
/// </summary>
public interface ICorrelationIdContext
{
    /// <summary>
    /// The unique correlation identifier for the current operation.
    /// </summary>
    string? CorrelationId { get; }
}
