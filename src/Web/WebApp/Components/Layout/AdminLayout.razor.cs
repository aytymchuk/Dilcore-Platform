using Dilcore.WebApp.Routing;
using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public partial class AdminLayout : TenantLayoutBase
{
    private string _searchText = string.Empty;
    private bool _drawerOpen = true;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private List<BreadcrumbItem> GetBreadcrumbs() =>
        BreadcrumbBuilder.Build(NavigationManager.Uri, Tenant, isAdmin: true);
}