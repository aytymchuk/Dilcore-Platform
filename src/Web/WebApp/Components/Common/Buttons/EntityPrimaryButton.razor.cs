using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Buttons;

public partial class EntityPrimaryButton
{
    [Parameter] public Variant Variant { get; set; } = Variant.Filled;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
}

