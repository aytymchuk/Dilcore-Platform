using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Common.Cards;

public partial class CreateEntityCard
{
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public EventCallback OnClick { get; set; }

    private async Task HandleClick()
    {
        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync();
        }
    }
}