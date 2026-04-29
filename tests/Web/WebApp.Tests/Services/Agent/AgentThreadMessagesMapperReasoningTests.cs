using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Services.Agent;
using NUnit.Framework;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Agent;

[TestFixture]
public class AgentThreadMessagesMapperReasoningTests
{
    [Test]
    public void Map_DuplicateEnvelopeIds_ShouldReplaceInsteadOfAppending()
    {
        var thread = new ThreadStateDto
        {
            Id = "t-1",
            Messages =
            [
                new MessageDto
                {
                    Id = "m-0",
                    Type = "human",
                    Content = "Hi"
                },
                new MessageDto
                {
                    Id = "m-1",
                    Type = "ai",
                    Content = "Hello"
                }
            ],
            Reasoning =
            [
                new ReasoningEnvelopeDto
                {
                    Id = "r-1",
                    Type = "reasoning",
                    AfterMessageId = "m-0",
                    Sequence = 1,
                    Node = "supervisor",
                    Header = "Header",
                    Steps =
                    [
                        new ReasoningStepDto { Kind = "step", Status = "running", Content = "A" }
                    ]
                },
                new ReasoningEnvelopeDto
                {
                    Id = "r-1",
                    Type = "reasoning",
                    AfterMessageId = "m-0",
                    Sequence = 2,
                    Node = "supervisor",
                    Header = "Header",
                    Steps =
                    [
                        new ReasoningStepDto { Kind = "step", Status = "completed", Content = "B" }
                    ]
                }
            ]
        };

        var mapped = AgentThreadMessagesMapper.Map(thread);
        mapped.Count.ShouldBe(2);

        var user = mapped[0];
        user.Reasoning.ShouldNotBeNull();
        user.Reasoning!.Sections.Count.ShouldBe(1);
        user.Reasoning.Sections[0].EnvelopeId.ShouldBe("r-1");
        user.Reasoning.Sections[0].Steps.Count.ShouldBe(1);
        user.Reasoning.Sections[0].Steps[0].Content.ShouldBe("B");
        user.Reasoning.Sections[0].Steps[0].Status.ShouldBe("completed");
    }
}

