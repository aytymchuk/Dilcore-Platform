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
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#1337ec",
            Secondary = "#14B8A6",
            Background = "#0b0f19",
            AppbarBackground = "#111422",
            DrawerBackground = "#0b0f19",
            Surface = "#151a2d",
            TextPrimary = "#ffffff",
            TextSecondary = "#929bc9",
            ActionDefault = "#1337ec",
        }
    };
}