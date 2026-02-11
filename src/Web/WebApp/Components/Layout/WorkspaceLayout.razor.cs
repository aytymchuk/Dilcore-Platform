using Dilcore.WebApp.Routing;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public partial class WorkspaceLayout : ThemeAwareLayoutBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private string _searchText = string.Empty;

    private string Tenant => ExtractTenant();

    private string? TenantName => !string.IsNullOrEmpty(Tenant) ? Tenant : null;

    private string ExtractTenant()
    {
        var uri = new Uri(NavigationManager.Uri);
        return TenantRouteHelper.ExtractTenantFromPath(uri.AbsolutePath) ?? string.Empty;
    }
}
