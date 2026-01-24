using MudBlazor;

namespace Dilcore.Platform.Web.WebApp.Components.Themes;

public static class FutureSlateTheme
{
    public static readonly MudTheme Default = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#334155", // Dark Slate
            Secondary = "#14B8A6", // Vibrant Teal (Accent)
            Background = "#F1F5F9", // Porcelain (Canvas)
            AppbarBackground = "#334155",
            DrawerBackground = "#FFFFFF",
            Surface = "#FFFFFF",
            TextPrimary = "#334155",
            ActionDefault = "#14B8A6",
        }
    };
}