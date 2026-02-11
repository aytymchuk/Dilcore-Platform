using Dilcore.WebApp.Routing;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class WorkspaceLayout : ThemeAwareLayoutBase, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private string _searchText = string.Empty;
    private string _tenant = string.Empty;

    private string Tenant => _tenant;

    private string? TenantName => !string.IsNullOrEmpty(Tenant) ? Tenant : null;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tenant = ExtractTenant();
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        var newTenant = ExtractTenant();
        if (_tenant != newTenant)
        {
            _tenant = newTenant;
            StateHasChanged();
        }
    }

    private string ExtractTenant()
    {
        var uri = new Uri(NavigationManager.Uri);
        return TenantRouteHelper.ExtractTenantFromPath(uri.AbsolutePath) ?? string.Empty;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
