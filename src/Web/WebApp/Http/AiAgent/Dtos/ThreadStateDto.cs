namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Full thread state returned by single-thread fetches or non-streaming agent actions, including message history.
/// Matches OpenAPI <c>ThreadResponseDto</c>.
/// </summary>
public sealed class ThreadStateDto
{
    /// <summary>
    /// Unique thread identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name when the API includes it on a full thread payload.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Ordered messages from the thread state.
    /// </summary>
    public IReadOnlyList<MessageDto> Messages { get; init; } = [];

    /// <summary>
    /// Persisted reasoning envelopes when returned by the API.
    /// </summary>
    public IReadOnlyList<ReasoningEnvelopeDto> Reasoning { get; init; } = [];
}
