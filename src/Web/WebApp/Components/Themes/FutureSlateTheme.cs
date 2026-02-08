using MudBlazor;

namespace Dilcore.WebApp.Components.Themes;

public static class FutureSlateTheme
{
    public static readonly MudTheme Default = CreateDefaultTheme();

    private static MudTheme CreateDefaultTheme()
    {
        var theme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#2563EB", // Primary Cobalt
                Secondary = "#0891B2", // Secondary Cyan
                Background = "#F8FAFF", // Soft Cobalt Tint
                AppbarBackground = "#FFFFFF",
                AppbarText = "#1e293b",
                DrawerBackground = "#FFFFFF",
                Surface = "#FFFFFF", // Surface Bright
                TextPrimary = "#1e293b", // Slate 800
                TextSecondary = "#64748b", // Slate 500
                ActionDefault = "#2563EB",
                Success = "#16A34A", // Validation
                Warning = "#D97706", // Cautionary
                Error = "#DC2626", // Destructive
                Info = "#2563EB"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#2563eb", // Primary Cobalt
                Secondary = "#22d3ee", // Secondary Cyan
                Background = "#0f172a", // Deep Navy/Cobalt
                AppbarBackground = "#0f172a", 
                AppbarText = "#ffffff",
                DrawerBackground = "#0f172a",
                Surface = "#1e293b", // Surface Container
                TextPrimary = "#ffffff", // On Primary / High Contrast
                TextSecondary = "#94a3b8", // Slate 400
                ActionDefault = "#2563eb",
                Success = "#10b981", // Validation
                Warning = "#f59e0b", // Cautionary
                Error = "#ef5350", // Destructive
                Info = "#2563eb"
            }
        };

        // Typography
        theme.Typography.Default.FontFamily = new[] { "Inter", "sans-serif" };

        // Custom Shadows
        theme.Shadows.Elevation[1] = "0px 1px 2px 0px rgba(0, 0, 0, 0.3), 0px 1px 3px 1px rgba(0, 0, 0, 0.15)";
        theme.Shadows.Elevation[2] = "0px 1px 2px 0px rgba(0, 0, 0, 0.3), 0px 2px 6px 2px rgba(0, 0, 0, 0.15)";
        theme.Shadows.Elevation[3] = "0px 4px 8px 3px rgba(0, 0, 0, 0.15), 0px 1px 3px 0px rgba(0, 0, 0, 0.3)";

        return theme;
    }
}