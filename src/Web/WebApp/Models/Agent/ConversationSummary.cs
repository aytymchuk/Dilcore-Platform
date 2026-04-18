namespace Dilcore.WebApp.Models.Agent;

public sealed record ConversationSummary(
    string Id,
    string Title,
    int MessageCount);
