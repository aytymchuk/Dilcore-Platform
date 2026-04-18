namespace Dilcore.WebApp.Models.Agent;

public enum ChatAuthor
{
    User,
    Assistant,
    Status
}

public record ChatMessage(
    Guid Id,
    ChatAuthor Author,
    string Content,
    DateTime Timestamp)
{
    /// <summary>Optional model reasoning text (streamed separately from the assistant reply).</summary>
    public string? Reasoning { get; init; }
}
