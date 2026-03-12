using Dilcore.WebApp.Constants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class WorkspaceNavMenu
{
    [Parameter]
    public string Tenant { get; set; } = string.Empty;

    private string _workspaceRoot => string.IsNullOrEmpty(Tenant) ? "/" : RouteConstants.Workspace.ForTenant(Tenant);
}
