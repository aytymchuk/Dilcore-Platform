using Dilcore.WebApp.Models.Agent;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ChatMessageBubble : ComponentBase
{
    [Parameter, EditorRequired]
    public ChatMessage Message { get; set; } = null!;

    [Parameter]
    public bool IsStreaming { get; set; }

    /// <summary>Live reasoning while streaming (overrides <see cref="ChatMessage.Reasoning"/>).</summary>
    [Parameter]
    public AgentReasoningProcess? ReasoningProcess { get; set; }

    private AgentReasoningProcess? EffectiveReasoning =>
        ReasoningProcess ?? Message.Reasoning;

    private static string FormatTime(DateTime timestamp) =>
        timestamp.ToLocalTime().ToString("h:mm tt");
}
