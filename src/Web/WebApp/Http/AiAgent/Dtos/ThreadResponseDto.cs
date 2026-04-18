namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Successful thread state returned by the blueprint agent API.
/// </summary>
public sealed class ThreadResponseDto
{
    /// <summary>
    /// Unique thread identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Ordered messages from the thread state.
    /// </summary>
    public required IReadOnlyList<MessageDto> Messages { get; init; }
}
