using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common;

public partial class CenteredCard
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string PaperMaxWidth { get; set; } = "440px";
    [Parameter] public MaxWidth ContainerMaxWidth { get; set; } = MaxWidth.Small;
}