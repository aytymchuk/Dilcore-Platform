using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Layout;

public abstract class ThemeAwareLayoutBase : LayoutComponentBase
{
    protected bool _isDarkMode = true;
    protected MudThemeProvider? _mudThemeProvider;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _mudThemeProvider is null)
        {
            return;
        }

        await _mudThemeProvider.WatchSystemDarkModeAsync(async (bool newValue) =>
        {
            _isDarkMode = newValue;
            await InvokeAsync(StateHasChanged);
        });

        StateHasChanged();
    }
}
