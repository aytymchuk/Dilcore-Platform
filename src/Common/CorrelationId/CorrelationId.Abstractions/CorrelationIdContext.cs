namespace Dilcore.CorrelationId.Abstractions;

/// <summary>
/// Immutable record representing resolved correlation ID context.
/// </summary>
public sealed record CorrelationIdContext(string? CorrelationId) : ICorrelationIdContext
{
    /// <summary>
    /// Represents a null/empty correlation ID context (no correlation ID resolved).
    /// </summary>
    public static readonly CorrelationIdContext Empty = new(correlationId: null);
}
