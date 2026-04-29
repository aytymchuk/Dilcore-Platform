using Dilcore.WebApp.Http.AiAgent.Streaming;
using Dilcore.WebApp.Services.Agent;

using FluentAssertions;

using NUnit.Framework;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Agent;

[TestFixture]
public class ThinkingReasoningAccumulatorTests
{
    [Test]
    public void Apply_SameEnvelopeAndEntryIndex_ShouldMergeUpdatesIntoSingleRow()
    {
        var acc = new ThinkingReasoningAccumulator();

        acc.Apply(new ThinkingStreamEvent
        {
            EnvelopeId = "r-1",
            EntryIndex = 0,
            Sequence = 10,
            AfterMessageId = "m-0",
            Node = "supervisor",
            Kind = "step",
            Status = "running",
            Content = "Working..."
        });

        acc.Apply(new ThinkingStreamEvent
        {
            EnvelopeId = "r-1",
            EntryIndex = 0,
            Sequence = 999,
            AfterMessageId = "m-0",
            Node = "supervisor",
            Kind = "step",
            Status = "completed",
            Content = "Done."
        });

        var proc = acc.ToProcess();

        proc.Sections.Count.ShouldBe(1);
        proc.Sections[0].Steps.Count.ShouldBe(1);
        proc.Sections[0].Steps[0].Status.ShouldBe("completed");
        proc.Sections[0].Steps[0].Content.ShouldBe("Done.");
    }

    [Test]
    public void Apply_SameEnvelopeAndEntryIndex_ShouldMergeItemsForNextSteps()
    {
        var acc = new ThinkingReasoningAccumulator();

        acc.Apply(new ThinkingStreamEvent
        {
            EnvelopeId = "r-1",
            EntryIndex = 1,
            Sequence = 10,
            AfterMessageId = "m-0",
            Node = "design",
            Kind = "next_steps",
            Status = "running",
            Items = ["First"]
        });

        acc.Apply(new ThinkingStreamEvent
        {
            EnvelopeId = "r-1",
            EntryIndex = 1,
            Sequence = 11,
            AfterMessageId = "m-0",
            Node = "design",
            Kind = "next_steps",
            Status = "completed",
            Items = ["First", "Second"]
        });

        var proc = acc.ToProcess();

        proc.Sections[0].Steps.Should().HaveCount(1);
        proc.Sections[0].Steps[0].Items.Should().Equal("First", "Second");
        proc.Sections[0].Steps[0].Status.Should().Be("completed");
    }
}

