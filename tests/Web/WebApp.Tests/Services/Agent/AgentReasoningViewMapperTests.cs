using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Services.Agent;

using FluentAssertions;

namespace Dilcore.WebApp.Tests.Services.Agent;

[TestFixture]
public class AgentReasoningViewMapperTests
{
    [Test]
    public void FromEnvelopes_ContentOnlyStep_Should_Map_Header_And_Content()
    {
        var envelopes = new List<ReasoningEnvelopeDto>
        {
            new()
            {
                Id = "r-1",
                Type = "reasoning",
                AfterMessageId = "m-0",
                Sequence = 1,
                Steps =
                [
                    new ReasoningStepDto
                    {
                        Kind = "step",
                        Status = "completed",
                        Header = null,
                        Content = "Checking which part of Blueprints can help best",
                        Items = null
                    }
                ]
            }
        };

        var proc = AgentReasoningViewMapper.FromEnvelopes(envelopes);

        proc.Sections.Should().HaveCount(1);
        proc.Sections[0].Steps.Should().HaveCount(1);
        proc.Sections[0].Steps[0].Header.Should().BeNull();
        proc.Sections[0].Steps[0].Content.Should().Be("Checking which part of Blueprints can help best");
        proc.Sections[0].Steps[0].Items.Should().BeNull();
    }

    [Test]
    public void FromEnvelopes_HeaderAndContent_Should_Map_Both()
    {
        var envelopes = new List<ReasoningEnvelopeDto>
        {
            new()
            {
                Id = "r-2",
                Type = "reasoning",
                AfterMessageId = "m-0",
                Sequence = 1,
                Steps =
                [
                    new ReasoningStepDto
                    {
                        Kind = "summary",
                        Status = "completed",
                        Header = "Summary title",
                        Content = "Longer rationale body.",
                        Items = null
                    }
                ]
            }
        };

        var proc = AgentReasoningViewMapper.FromEnvelopes(envelopes);

        var step = proc.Sections[0].Steps[0];
        step.Header.Should().Be("Summary title");
        step.Content.Should().Be("Longer rationale body.");
    }

    [Test]
    public void FromEnvelopes_NextSteps_Should_Map_Items()
    {
        var envelopes = new List<ReasoningEnvelopeDto>
        {
            new()
            {
                Id = "r-3",
                Type = "reasoning",
                AfterMessageId = "m-0",
                Sequence = 1,
                Steps =
                [
                    new ReasoningStepDto
                    {
                        Kind = "next_steps",
                        Status = "completed",
                        Header = null,
                        Content = null,
                        Items = ["If you ask for generation next, we'll use these notes in the plan."]
                    }
                ]
            }
        };

        var proc = AgentReasoningViewMapper.FromEnvelopes(envelopes);

        proc.Sections[0].Steps[0].Kind.Should().Be("next_steps");
        proc.Sections[0].Steps[0].Items.Should().Equal("If you ask for generation next, we'll use these notes in the plan.");
    }

    [Test]
    public void FromEnvelopes_MultipleEnvelopes_Should_Order_By_Sequence()
    {
        var envelopes = new List<ReasoningEnvelopeDto>
        {
            new()
            {
                Id = "r-b",
                Type = "reasoning",
                AfterMessageId = "m-0",
                Sequence = 2,
                Header = "Second",
                Steps = []
            },
            new()
            {
                Id = "r-a",
                Type = "reasoning",
                AfterMessageId = "m-0",
                Sequence = 1,
                Header = "First",
                Steps = []
            }
        };

        var proc = AgentReasoningViewMapper.FromEnvelopes(envelopes);

        proc.Sections.Should().HaveCount(2);
        proc.Sections[0].Header.Should().Be("First");
        proc.Sections[1].Header.Should().Be("Second");
    }
}
