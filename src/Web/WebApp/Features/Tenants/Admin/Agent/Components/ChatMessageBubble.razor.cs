using Dilcore.WebApp.Models.Agent;
using Dilcore.WebApp.Services.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ChatMessageBubble : ComponentBase
{
    [Inject]
    private IMarkdownRenderer Markdown { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Parameter, EditorRequired]
    public ChatMessage Message { get; set; } = null!;

    [Parameter]
    public bool IsStreaming { get; set; }

    /// <summary>Live reasoning buffer while streaming (assistant bubble only).</summary>
    [Parameter]
    public string? Reasoning { get; set; }

    private MarkupString MarkdownHtml { get; set; }

    private bool _reasoningExpanded;

    private string? ReasoningDisplayText =>
        Reasoning ?? Message.Reasoning;

    private bool ShowReasoningSection =>
        Message.Author == ChatAuthor.Assistant
        && !string.IsNullOrEmpty(ReasoningDisplayText);

    protected override void OnParametersSet()
    {
        MarkdownHtml = Message.Author == ChatAuthor.Assistant
            ? Markdown.ToHtml(Message.Content)
            : default;

        if (IsStreaming && !string.IsNullOrEmpty(ReasoningDisplayText))
        {
            _reasoningExpanded = true;
        }
    }

    private static string FormatTime(DateTime timestamp) =>
        timestamp.ToLocalTime().ToString("h:mm tt");

    private async Task CopyAsync()
    {
        await Js.InvokeVoidAsync("dilcoreAgent.copyText", Message.Content);
    }
}
