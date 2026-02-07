using MudBlazor;

namespace Dilcore.Platform.Web.WebApp.Components.Themes;

public static class FutureSlateTheme
{
    public static readonly MudTheme Default = new MudTheme()
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
}