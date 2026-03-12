using Dilcore.WebApp.Models.Tenants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Admin;

public partial class AdminHome
{
    [CascadingParameter]
    public TenantState? TenantContext { get; set; }
}
