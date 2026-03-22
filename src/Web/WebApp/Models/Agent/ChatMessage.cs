namespace Dilcore.WebApp.Models.Agent;

public record ChatMessage(
    Guid Id,
    string Content,
    bool IsUser,
    DateTime Timestamp);
