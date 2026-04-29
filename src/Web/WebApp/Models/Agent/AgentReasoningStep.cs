namespace Dilcore.WebApp.Models.Agent;

/// <summary>
/// Single displayed row in a reasoning process (shared across agent UIs).
/// </summary>
public sealed class AgentReasoningStep
{
    public string? Kind { get; init; }

    public string? Status { get; init; }

    /// <summary>Short label from API when present; optional longer text in <see cref="Content"/>.</summary>
    public string? Header { get; init; }

    public string Content { get; init; } = string.Empty;

    /// <summary>Bullet lines for <c>next_steps</c> rows.</summary>
    public IReadOnlyList<string>? Items { get; init; }

    public string? DetailMarkdown { get; init; }

    public string? Node { get; init; }

    public string? AgentType { get; init; }

    public int? Sequence { get; init; }

    public string? DurationText { get; init; }
}
