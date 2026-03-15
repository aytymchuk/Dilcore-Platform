using Dilcore.WebApp.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Dilcore.WebApp.Components.Layout;

/// <summary>
/// Base class for tenant-aware layouts that handles theme and tenant extraction from URL.
/// </summary>
public abstract class TenantLayoutBase : ThemeAwareLayoutBase, IDisposable
{
    private string _tenant = string.Empty;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected string Tenant => _tenant;

    protected string? TenantName => !string.IsNullOrEmpty(Tenant) ? Tenant : null;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tenant = ExtractTenant();
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    protected void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var newTenant = ExtractTenant();
        if (_tenant != newTenant)
        {
            _tenant = newTenant;
        }

        StateHasChanged();
    }

    protected string ExtractTenant()
    {
        var uri = new Uri(NavigationManager.Uri);
        return TenantRouteHelper.ExtractTenantFromPath(uri.AbsolutePath) ?? string.Empty;
    }
}
