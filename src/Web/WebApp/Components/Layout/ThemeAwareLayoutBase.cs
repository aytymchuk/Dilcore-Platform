using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public abstract class ThemeAwareLayoutBase : LayoutComponentBase
{
    protected bool _isDarkMode = true;
    protected MudThemeProvider _mudThemeProvider = null!;

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
