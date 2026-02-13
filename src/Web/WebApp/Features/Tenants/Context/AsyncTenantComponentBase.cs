using Dilcore.WebApp.Components.Common;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Context;

/// <summary>
/// Base component for tenant-aware pages that extracts tenant from route and provides it to layouts.
/// </summary>
public abstract class AsyncTenantComponentBase : AsyncComponentBase
{
    [Parameter]
    public string Tenant { get; set; } = string.Empty;
}
