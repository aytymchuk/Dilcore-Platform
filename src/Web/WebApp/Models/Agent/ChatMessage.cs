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
    /// <summary>Optional stable id from the agent API for reasoning anchors.</summary>
    public string? ApiMessageId { get; init; }

    /// <summary>Optional reasoning UI payload.</summary>
    public AgentReasoningProcess? Reasoning { get; init; }
}
