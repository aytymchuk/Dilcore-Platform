using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Features.Tenants.Context;
using Dilcore.WebApp.Models.Agent;

using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent;

public partial class AdminAgent : AsyncTenantComponentBase
{
    private readonly List<AgentQuickAction> _quickActions = BuildQuickActions();

    private string _inputText = string.Empty;

    public async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key != "Enter" || e.ShiftKey)
        {
            return;
        }

        await HandleSendAsync();
    }

    public Task HandleSendAsync()
    {
        var text = _inputText.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return Task.CompletedTask;
        }

        _inputText = string.Empty;
        StateHasChanged();

        return Task.CompletedTask;
    }

    private void HandleQuickActionClick(AgentQuickAction action)
    {
        _inputText = action.Prompt;
        StateHasChanged();
    }

    private static List<AgentQuickAction> BuildQuickActions()
    {
        return
        [
            new AgentQuickAction(
                Icons.Material.Filled.Schema,
                AgentConstants.QuickActions.BuildDataSchemaTitle,
                AgentConstants.QuickActions.BuildDataSchemaDescription,
                AgentConstants.QuickActions.BuildDataSchemaPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.AutoFixHigh,
                AgentConstants.QuickActions.AutomateLogicTitle,
                AgentConstants.QuickActions.AutomateLogicDescription,
                AgentConstants.QuickActions.AutomateLogicPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.Speed,
                AgentConstants.QuickActions.PerformanceAuditTitle,
                AgentConstants.QuickActions.PerformanceAuditDescription,
                AgentConstants.QuickActions.PerformanceAuditPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.MenuBook,
                AgentConstants.QuickActions.SystemGuideTitle,
                AgentConstants.QuickActions.SystemGuideDescription,
                AgentConstants.QuickActions.SystemGuidePrompt)
        ];
    }
}
