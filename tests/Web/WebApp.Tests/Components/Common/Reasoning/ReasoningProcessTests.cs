using Bunit;
using Dilcore.WebApp.Components.Common.Reasoning;
using Dilcore.WebApp.Models.Agent;

using FluentAssertions;

using MudBlazor;
using MudBlazor.Services;

namespace Dilcore.WebApp.Tests.Components.Common.Reasoning;

[TestFixture]
public class ReasoningProcessTests : Bunit.TestContext
{
    public ReasoningProcessTests()
    {
        Services.AddMudServices();
        Services.AddMudMarkdownServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Test]
    public void ReasoningProcess_Summary_Step_Should_Render_Summary_Block_And_Strong_From_Markdown()
    {
        var process = new AgentReasoningProcess
        {
            Title = "Agent Thinking",
            Sections =
            [
                new AgentReasoningSection
                {
                    Steps =
                    [
                        new AgentReasoningStep
                        {
                            Kind = "step",
                            Status = "completed",
                            Header = "Step one",
                            Content = "Detail **one**",
                        },
                        new AgentReasoningStep
                        {
                            Kind = "summary",
                            Status = "completed",
                            Header = "Summary title",
                            Content = "Wrapped **summary** body.",
                        },
                    ],
                },
            ],
        };

        var cut = RenderComponent<ReasoningProcess>(parameters => parameters
            .Add(p => p.Process, process)
            .Add(p => p.InitiallyExpanded, true));

        cut.FindAll(".reasoning-summary").Count.Should().Be(1);
        cut.Markup.Should().Contain("<b>summary</b>");
        cut.Markup.Should().Contain("reasoning-summary");
        cut.Markup.Should().Contain("mud-expand-panel");
    }

    [Test]
    public void ReasoningProcess_Step_Count_Chip_Should_Exclude_Summary_Steps()
    {
        var process = new AgentReasoningProcess
        {
            Title = "T",
            Sections =
            [
                new AgentReasoningSection
                {
                    Steps =
                    [
                        new AgentReasoningStep { Kind = "step", Status = "completed", Content = "A" },
                        new AgentReasoningStep { Kind = "summary", Status = "completed", Content = "S" },
                    ],
                },
            ],
        };

        var cut = RenderComponent<ReasoningProcess>(parameters => parameters
            .Add(p => p.Process, process)
            .Add(p => p.InitiallyExpanded, true));

        cut.Markup.Should().Contain("1 steps");
        cut.Markup.Should().NotContain("2 steps");
    }
}
