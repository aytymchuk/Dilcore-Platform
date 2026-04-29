namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Response returned when a graph interrupt is pending (matches OpenAPI <see cref="InterruptResponseDto"/>).
/// </summary>
public sealed class InterruptResponseDto
{
    /// <summary>
    /// Thread identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Pending interrupts requiring user action.
    /// </summary>
    public required IReadOnlyList<InterruptDto> Interrupts { get; init; }

    /// <summary>
    /// Messages produced before the interrupt.
    /// </summary>
    public IReadOnlyList<MessageDto>? Messages { get; init; }

    /// <summary>
    /// Persisted reasoning envelopes when returned by the API.
    /// </summary>
    public IReadOnlyList<ReasoningEnvelopeDto>? Reasoning { get; init; }
}
