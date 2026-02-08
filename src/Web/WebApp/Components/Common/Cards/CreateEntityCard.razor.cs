using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Common.Cards;

public partial class CreateEntityCard
{
    [Parameter] public EventCallback OnClick { get; set; }

    private async Task HandleClick()
    {
        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync();
        }
    }
}