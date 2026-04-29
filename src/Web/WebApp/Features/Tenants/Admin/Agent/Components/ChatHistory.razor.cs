using Dilcore.WebApp.Models.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ChatHistory : ComponentBase
{
    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Parameter]
    public string? ThreadTitle { get; set; }

    [Parameter]
    public IReadOnlyList<ChatMessage> Messages { get; set; } = [];

    [Parameter]
    public string? PendingAssistantMarkdown { get; set; }

    [Parameter]
    public AgentReasoningProcess? PendingReasoningProcess { get; set; }

    [Parameter]
    public bool IsStreaming { get; set; }

    [Parameter]
    public string? Status { get; set; }

    private ElementReference _scrollRegion;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Messages.Count == 0 && string.IsNullOrEmpty(PendingAssistantMarkdown) && PendingReasoningProcess is null && !IsStreaming)
        {
            return;
        }

        try
        {
            await Js.InvokeVoidAsync("dilcoreAgent.scrollContainerToEnd", _scrollRegion);
        }
        catch
        {
            // Ignore scroll errors (e.g. prerender or SSR without JS).
        }
    }
}
