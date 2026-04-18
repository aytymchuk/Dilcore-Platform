namespace Dilcore.WebApp.Services.Tenancy;

/// <summary>
/// Tenant system name for the current Blazor circuit, set by TenantStateProvider.
/// </summary>
public interface IBlazorTenantContext
{
    string? SystemName { get; }

    void Set(string systemName);
}

internal sealed class BlazorTenantContext : IBlazorTenantContext
{
    public string? SystemName { get; private set; }

    public void Set(string systemName) => SystemName = systemName;
}
