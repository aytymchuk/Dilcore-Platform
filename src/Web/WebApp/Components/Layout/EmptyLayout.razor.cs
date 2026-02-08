using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public partial class EmptyLayout : LayoutComponentBase
{
    bool _isDarkMode = true;
    MudThemeProvider _mudThemeProvider = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _mudThemeProvider.WatchSystemDarkModeAsync(async (bool newValue) =>
            {
                _isDarkMode = newValue;
                await InvokeAsync(StateHasChanged);
            });
            StateHasChanged();
        }
    }
}