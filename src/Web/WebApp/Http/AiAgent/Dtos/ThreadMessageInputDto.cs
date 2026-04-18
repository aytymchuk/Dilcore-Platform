namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Input DTO for blueprint agent thread messages.
/// </summary>
public sealed class ThreadMessageInputDto
{
    /// <summary>
    /// The user message (1–4000 characters).
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional base64-encoded file content (currently ignored by the API).
    /// </summary>
    public string? File { get; init; }
}
