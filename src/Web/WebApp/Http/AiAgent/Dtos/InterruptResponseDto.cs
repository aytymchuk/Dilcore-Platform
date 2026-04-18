using System.Text.Json;

namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Response when the agent graph pauses for user confirmation or input.
/// </summary>
public sealed class InterruptResponseDto
{
    /// <summary>
    /// Thread identifier when provided by the API.
    /// </summary>
    public string? ThreadId { get; init; }

    /// <summary>
    /// Raw interrupt payload for forward-compatibility with API schema changes.
    /// </summary>
    public JsonElement? Payload { get; init; }
}
