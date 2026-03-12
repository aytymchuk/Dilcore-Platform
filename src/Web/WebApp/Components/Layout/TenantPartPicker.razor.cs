using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class TenantPartPicker
{
    [Parameter]
    public string CurrentPart { get; set; } = "Workspace";

    [Parameter]
    public string Tenant { get; set; } = string.Empty;
}
