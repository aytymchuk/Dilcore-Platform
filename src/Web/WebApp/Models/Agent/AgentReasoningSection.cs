namespace Dilcore.WebApp.Models.Agent;

/// <summary>
/// Group of steps under an optional header (one persisted reasoning envelope).
/// </summary>
public sealed class AgentReasoningSection
{
    public string? EnvelopeId { get; init; }

    public string? Header { get; init; }

    public string? Node { get; init; }

    public string? AgentType { get; init; }

    public IReadOnlyList<AgentReasoningStep> Steps { get; init; } = [];
}
