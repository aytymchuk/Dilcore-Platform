using Bunit;
using Dilcore.WebApp.Components.Common.Reasoning;
using Dilcore.WebApp.Models.Agent;

using FluentAssertions;

using MudBlazor;
using MudBlazor.Services;

namespace Dilcore.WebApp.Tests.Components.Common.Reasoning;

[TestFixture]
public class ReasoningStepRowTests : Bunit.TestContext
{
    public ReasoningStepRowTests()
    {
        Services.AddMudServices();
        Services.AddMudMarkdownServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Test]
    public void ReasoningStepRow_ContentOnlyLong_Should_Show_Truncated_Title_With_Ellipsis()
    {
        var longContent = new string('a', 35);

        var cut = RenderComponent<ReasoningStepRow>(parameters => parameters
            .Add(p => p.Step, new AgentReasoningStep
            {
                Status = "completed",
                Content = longContent,
            })
            .Add(p => p.InitiallyExpanded, true));

        cut.Markup.Should().Contain($"{new string('a', 30)}...");
    }

    [Test]
    public void ReasoningStepRow_With_Header_And_Markdown_Content_Should_Render_Strong_From_Markdown()
    {
        var cut = RenderComponent<ReasoningStepRow>(parameters => parameters
            .Add(p => p.Step, new AgentReasoningStep
            {
                Status = "completed",
                Header = "Step header",
                Content = "**bold** label",
            })
            .Add(p => p.InitiallyExpanded, true));

        cut.Markup.Should().Contain("<b>bold</b>");
    }

    [Test]
    public void ReasoningStepRow_NextSteps_ItemsOnly_Should_Not_Use_Expansion_Panel()
    {
        var cut = RenderComponent<ReasoningStepRow>(parameters => parameters
            .Add(p => p.Step, new AgentReasoningStep
            {
                Kind = "next_steps",
                Status = "completed",
                Items = ["First", "Second"],
            })
            .Add(p => p.InitiallyExpanded, false));

        cut.Markup.Should().NotContain("mud-expand-panel");
        cut.Markup.Should().Contain("First");
        cut.Markup.Should().Contain("Second");
    }
}
