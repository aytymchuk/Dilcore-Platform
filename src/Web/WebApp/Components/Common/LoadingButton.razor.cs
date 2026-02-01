using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common;

/// <summary>
/// Code-behind for LoadingButton component.
/// Wrapper around MudButton to handle loading states.
/// </summary>
public partial class LoadingButton
{
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? LoadingText { get; set; } = "PROCESSING";
    [Parameter] public string? Text { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    // MudButton passthrough parameters
    [Parameter] public Variant Variant { get; set; } = Variant.Filled;
    [Parameter] public Color Color { get; set; } = Color.Primary;
    [Parameter] public Size Size { get; set; } = Size.Large;
    [Parameter] public bool FullWidth { get; set; } = true;
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
}