using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Dilcore.WebApp.Models.Agent;

namespace Dilcore.WebApp.Services.Agent;

/// <summary>
/// Builds live <see cref="AgentReasoningProcess"/> from streamed <see cref="ThinkingStreamEvent"/> fragments.
/// </summary>
public sealed class ThinkingReasoningAccumulator
{
    private readonly List<AgentReasoningStep> _steps = [];

    private readonly Dictionary<string, int> _indexByKey = [];

    public void Apply(ThinkingStreamEvent e)
    {
        var key = BuildKey(e);
        var next = ToStep(e);

        if (_indexByKey.TryGetValue(key, out var idx))
        {
            _steps[idx] = Merge(_steps[idx], next);
        }
        else
        {
            _indexByKey[key] = _steps.Count;
            _steps.Add(next);
        }
    }

    public void Clear()
    {
        _steps.Clear();
        _indexByKey.Clear();
    }

    public AgentReasoningProcess ToProcess()
    {
        var orderedSteps = _steps
            .OrderBy(s => s.Sequence ?? int.MaxValue)
            .ThenBy(s => s.Node)
            .ThenBy(s => s.Kind)
            .ToList();

        return new AgentReasoningProcess
        {
            Title = AgentConstants.ReasoningLabel,
            Sections =
            [
                new AgentReasoningSection
                {
                    Steps = orderedSteps
                }
            ]
        };
    }

    private static string BuildKey(ThinkingStreamEvent e)
    {
        if (!string.IsNullOrEmpty(e.EnvelopeId) && e.EntryIndex is not null)
        {
            return $"env:{e.EnvelopeId}|idx:{e.EntryIndex.Value}";
        }

        var seq = e.Sequence?.ToString() ?? "null";
        var after = e.AfterMessageId ?? string.Empty;
        var node = e.Node ?? string.Empty;
        var kind = e.Kind ?? string.Empty;
        return $"{seq}|{after}|{node}|{kind}";
    }

    private static AgentReasoningStep ToStep(ThinkingStreamEvent e)
    {
        return new AgentReasoningStep
        {
            Kind = e.Kind,
            Status = e.Status,
            Header = e.Header,
            Content = e.Content ?? string.Empty,
            Items = e.Items,
            Node = e.Node,
            AgentType = e.AgentType,
            Sequence = e.Sequence
        };
    }

    private static AgentReasoningStep Merge(AgentReasoningStep previous, AgentReasoningStep next)
    {
        return new AgentReasoningStep
        {
            Kind = next.Kind ?? previous.Kind,
            Status = next.Status ?? previous.Status,
            Header = string.IsNullOrEmpty(next.Header) ? previous.Header : next.Header,
            Content = string.IsNullOrEmpty(next.Content) ? previous.Content : next.Content,
            Items = next.Items ?? previous.Items,
            DetailMarkdown = next.DetailMarkdown ?? previous.DetailMarkdown,
            Node = next.Node ?? previous.Node,
            AgentType = next.AgentType ?? previous.AgentType,
            Sequence = next.Sequence ?? previous.Sequence,
            DurationText = next.DurationText ?? previous.DurationText
        };
    }
}
