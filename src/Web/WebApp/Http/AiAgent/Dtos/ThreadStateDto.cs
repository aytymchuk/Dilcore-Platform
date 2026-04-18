namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Full thread state returned by single-thread fetches or non-streaming agent actions, including message history.
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
    public required IReadOnlyList<MessageDto> Messages { get; init; }
}
