namespace Dilcore.WebApp.Services;

/// <summary>
/// Provides access to the current tenant name within a Blazor circuit.
/// </summary>
public interface IBlazorTenantAccessor
{
    string? TenantName { get; set; }
}

/// <summary>
/// Accessor for the current tenant name using AsyncLocal to flow state
/// across DI scopes (e.g., from Blazor components into IHttpClientFactory handlers).
/// </summary>
public class BlazorTenantAccessor : IBlazorTenantAccessor
{
    private static readonly AsyncLocal<string?> _tenantName = new();

    public string? TenantName
    {
        get => _tenantName.Value;
        set => _tenantName.Value = value;
    }
}
