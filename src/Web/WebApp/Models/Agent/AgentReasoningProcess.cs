namespace Dilcore.WebApp.Models.Agent;

/// <summary>
/// Full reasoning card payload for shared UI components.
/// </summary>
public sealed class AgentReasoningProcess
{
    public string Title { get; init; } = string.Empty;

    public string? Subtitle { get; init; }

    public string? TotalDurationText { get; init; }

    public bool CanCopyLogs { get; init; }

    public string? CopyLogsText { get; init; }

    public IReadOnlyList<AgentReasoningSection> Sections { get; init; } = [];

    public int StepCount =>
        Sections.Sum(s => s.Steps.Count);
}
