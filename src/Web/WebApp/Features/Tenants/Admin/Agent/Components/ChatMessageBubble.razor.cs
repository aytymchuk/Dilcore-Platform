using Dilcore.WebApp.Models.Agent;
using Dilcore.WebApp.Services.Agent;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ChatMessageBubble : ComponentBase
{
    [Parameter, EditorRequired]
    public ChatMessage Message { get; set; } = null!;

    [Parameter]
    public bool IsStreaming { get; set; }

    /// <summary>Live reasoning buffer while streaming (assistant bubble only).</summary>
    [Parameter]
    public string? Reasoning { get; set; }

    private bool _reasoningExpanded;

    private AgentReasoningDisplay _reasoningDisplay;

    private string? ReasoningDisplayText =>
        Reasoning ?? Message.Reasoning;

    private bool ShowReasoningSection =>
        Message.Author == ChatAuthor.Assistant
        && !string.IsNullOrEmpty(ReasoningDisplayText);

    protected override void OnParametersSet()
    {
        _reasoningDisplay = AgentReasoningPayload.Parse(ReasoningDisplayText);

        if (IsStreaming && !string.IsNullOrEmpty(ReasoningDisplayText))
        {
            _reasoningExpanded = true;
        }
    }

    private static string FormatTime(DateTime timestamp) =>
        timestamp.ToLocalTime().ToString("h:mm tt");
}
