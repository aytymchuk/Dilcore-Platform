using Dilcore.WebApp.Components.Common.Reasoning;
using Dilcore.WebApp.Models.Agent;

using FluentAssertions;

namespace Dilcore.WebApp.Tests.Components.Common.Reasoning;

[TestFixture]
public class ReasoningStepDisplayHelperTests
{
    [Test]
    public void TruncateForStepTitle_Should_Return_Empty_For_Null_Or_Whitespace()
    {
        ReasoningStepDisplayHelper.TruncateForStepTitle(null).Should().Be(string.Empty);
        ReasoningStepDisplayHelper.TruncateForStepTitle("   ").Should().Be(string.Empty);
    }

    [Test]
    public void TruncateForStepTitle_Should_Not_Truncate_When_At_Or_Under_Limit()
    {
        var s = new string('x', 30);
        ReasoningStepDisplayHelper.TruncateForStepTitle(s).Should().Be(s);
        ReasoningStepDisplayHelper.TruncateForStepTitle("short").Should().Be("short");
    }

    [Test]
    public void TruncateForStepTitle_Should_Truncate_To_30_And_Append_Ellipsis()
    {
        var content = new string('a', 35);
        ReasoningStepDisplayHelper.TruncateForStepTitle(content).Should().Be($"{new string('a', 30)}...");
    }

    [Test]
    public void TruncateForStepTitle_Should_Trim_Leading_And_Trailing_Whitespace()
    {
        ReasoningStepDisplayHelper.TruncateForStepTitle("   abc   ").Should().Be("abc");
    }

    [Test]
    public void IsSummaryKind_Should_Be_True_When_Kind_Is_Summary_IgnoreCase()
    {
        var step = new AgentReasoningStep { Kind = "summary" };
        ReasoningStepDisplayHelper.IsSummaryKind(step).Should().BeTrue();

        step = new AgentReasoningStep { Kind = "SUMMARY" };
        ReasoningStepDisplayHelper.IsSummaryKind(step).Should().BeTrue();
    }

    [Test]
    public void IsSummaryKind_Should_Be_False_For_Other_Kinds()
    {
        var step = new AgentReasoningStep { Kind = "step" };
        ReasoningStepDisplayHelper.IsSummaryKind(step).Should().BeFalse();
    }
}
