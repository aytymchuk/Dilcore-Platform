using Dilcore.WebApp.Models.Tenants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Workspace;

public partial class WorkspaceHome
{
    [CascadingParameter]
    public TenantState? TenantContext { get; set; }
}
