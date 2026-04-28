using MudBlazor;

namespace Dilcore.WebApp.Components.Themes;

public static class FutureSlateTheme
{
    public static readonly MudTheme Default = CreateDefaultTheme();

    private static MudTheme CreateDefaultTheme()
    {
        var theme = new MudTheme()
        {
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "0.75rem",
            },
            PaletteLight = new PaletteLight()
            {
                Primary = "#5565dd",
                Secondary = "#414141",
                Background = "#FFFFFF",
                Surface = "#F9FAFB",
                AppbarBackground = "#F9FAFB",
                AppbarText = "#0F172A",
                DrawerBackground = "#F9FAFB",
                DrawerText = "#0F172A",
                DrawerIcon = "#64748B",
                TextPrimary = "#0F172A",
                TextSecondary = "#64748B",
                ActionDefault = "#0F172A",
                ActionDisabled = "#E4E4E7",
                ActionDisabledBackground = "#F9FAFB",
                Divider = "#E4E4E7",
                DividerLight = "#E4E4E7",
                TableLines = "#E4E4E7",
                LinesDefault = "#E4E4E7",
                LinesInputs = "#E4E4E7",
                Success = "#10B981",
                Warning = "#F59E0B",
                Error = "#EF4444",
                Info = "#3B82F6",
                Black = "#000000",
                White = "#FFFFFF"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#5565dd",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#414141",
                Background = "#09090B",
                AppbarBackground = "#09090B",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#09090B",
                DrawerText = "#A1A1AA",
                DrawerIcon = "#A1A1AA",
                Surface = "#18181B",
                TextPrimary = "#FAFAFA",
                TextSecondary = "#A1A1AA",
                ActionDefault = "#A1A1AA",
                ActionDisabled = "#27272A",
                ActionDisabledBackground = "#121212",
                Divider = "#27272A",
                DividerLight = "#27272A",
                TableLines = "#27272A",
                LinesDefault = "#27272A",
                LinesInputs = "#27272A",
                Success = "#10B981",
                Warning = "#F59E0B",
                Error = "#EF4444",
                Info = "#3B82F6",
                Black = "#000000",
                White = "#FFFFFF"
            }
        };

        // Typography
        theme.Typography.Default.FontFamily = new[] { "Inter", "sans-serif" };
        theme.Typography.Default.FontSize = "0.875rem";
        theme.Typography.Default.FontWeight = "400";
        theme.Typography.Default.LineHeight = "1.43";
        theme.Typography.Default.LetterSpacing = ".01071em";

        theme.Typography.H1.FontSize = "6rem";
        theme.Typography.H1.FontWeight = "300";
        theme.Typography.H1.LineHeight = "1.167";
        theme.Typography.H1.LetterSpacing = "-.01562em";

        theme.Typography.Button.TextTransform = "none";

        // Custom Shadows (Shadowless Elevation)
        // We simulate a 1px border using box-shadow to avoid layout shifts and ensure compatibility with MudPaper's Elevation property.
        string shadowlessBorder = "0px 0px 0px 1px var(--mud-palette-lines-default)";

        for (int i = 1; i < 25; i++)
        {
            theme.Shadows.Elevation[i] = shadowlessBorder;
        }

        return theme;
    }
}