namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// A message in a blueprint agent thread.
/// </summary>
public sealed class MessageDto
{
    /// <summary>
    /// Message type discriminator.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Message body text.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Optional agent type identifier.
    /// </summary>
    public string? AgentType { get; init; }
}
