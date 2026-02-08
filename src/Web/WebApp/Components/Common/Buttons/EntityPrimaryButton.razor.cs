using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Buttons;

public partial class EntityPrimaryButton
{
    [Parameter] public string ButtonText { get; set; } = "Select";
    [Parameter] public string? ButtonIcon { get; set; }
    [Parameter] public Color ButtonColor { get; set; } = Color.Primary;
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public Size Size { get; set; } = Size.Large;
}
