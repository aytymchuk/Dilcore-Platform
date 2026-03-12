using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public partial class AdminLayout : TenantLayoutBase
{
    private readonly List<BreadcrumbItem> _breadcrumbs = new()
    {
        new BreadcrumbItem("Administration", href: "#"),
        new BreadcrumbItem("Blueprints", href: "#"),
        new BreadcrumbItem("Entities", href: null, disabled: true)
    };

    private string _searchText = string.Empty;
    private bool _drawerOpen = true;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}