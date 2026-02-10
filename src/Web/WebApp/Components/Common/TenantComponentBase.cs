using Dilcore.WebApp.Models.Tenants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Common;

/// <summary>
/// Base component for tenant-aware pages that provides access to the current tenant context.
/// </summary>
public abstract class TenantComponentBase : AsyncComponentBase
{
    /// <summary>
    /// Gets the current tenant state from the cascading parameter.
    /// </summary>
    [CascadingParameter]
    public TenantState? TenantState { get; set; }

    /// <summary>
    /// Gets the system name of the current tenant, or null if no tenant is set.
    /// </summary>
    protected string? TenantSystemName => TenantState?.SystemName;

    /// <summary>
    /// Gets the display name of the current tenant, or null if no tenant is set.
    /// </summary>
    protected string? TenantName => TenantState?.Name;
}
