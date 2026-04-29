using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Models.Agent;

namespace Dilcore.WebApp.Services.Agent;

/// <summary>
/// Maps API reasoning DTOs to shared <see cref="AgentReasoningProcess"/> view models.
/// </summary>
public static class AgentReasoningViewMapper
{
    public static AgentReasoningProcess FromEnvelopes(IReadOnlyList<ReasoningEnvelopeDto>? envelopes)
    {
        if (envelopes is null || envelopes.Count == 0)
        {
            return new AgentReasoningProcess
            {
                Title = AgentConstants.ReasoningLabel
            };
        }

        var ordered = envelopes.OrderBy(e => e.Sequence).ToList();
        var sections = new List<AgentReasoningSection>(ordered.Count);
        foreach (var env in ordered)
        {
            sections.Add(new AgentReasoningSection
            {
                EnvelopeId = env.Id,
                Header = env.Header,
                Node = env.Node,
                AgentType = env.AgentType,
                Steps = MapSteps(env.Steps)
            });
        }

        return new AgentReasoningProcess
        {
            Title = AgentConstants.ReasoningLabel,
            Sections = sections
        };
    }

    private static IReadOnlyList<AgentReasoningStep> MapSteps(IReadOnlyList<ReasoningStepDto>? steps)
    {
        if (steps is null || steps.Count == 0)
        {
            return [];
        }

        return steps.Select(s => new AgentReasoningStep
        {
            Kind = s.Kind,
            Status = s.Status,
            Header = s.Header,
            Content = s.Content ?? string.Empty,
            Items = s.Items,
            DetailMarkdown = null
        }).ToList();
    }
}
