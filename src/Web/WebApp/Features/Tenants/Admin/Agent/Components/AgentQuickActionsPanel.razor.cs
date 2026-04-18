using Dilcore.WebApp.Models.Agent;

using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class AgentQuickActionsPanel
{
    [Parameter]
    public IReadOnlyList<AgentQuickAction> QuickActions { get; set; } = [];

    [Parameter]
    public EventCallback<AgentQuickAction> OnActionSelected { get; set; }
}
