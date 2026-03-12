using Dilcore.WebApp.Constants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class AdminNavMenu
{
    [Parameter]
    public string Tenant { get; set; } = string.Empty;

    private string _adminRoot => string.IsNullOrEmpty(Tenant) ? "/" : RouteConstants.Workspace.Admin.ForTenant(Tenant);
}
