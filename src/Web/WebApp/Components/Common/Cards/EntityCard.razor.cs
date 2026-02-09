using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Cards;

public partial class EntityCard
{
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public string SubtitleIcon { get; set; } = Icons.Material.Outlined.Dns;
    [Parameter] public string Description { get; set; } = "";
    
    [Parameter] public string? Label { get; set; }
    [Parameter] public Color LabelColor { get; set; } = Color.Default;
    [Parameter] public string? LabelBackgroundColor { get; set; }
    [Parameter] public string? LabelBorderColor { get; set; }
    [Parameter] public string? LabelTextColor { get; set; }

    [Parameter] public string GradientStart { get; set; } = "#1e3a8a";
    [Parameter] public string GradientEnd { get; set; } = "#0f172a";
    [Parameter] public string PatternOverlayStyle { get; set; } = "";
    
    [Parameter] public string ButtonText { get; set; } = "Select";
    [Parameter] public string ButtonIcon { get; set; } = Icons.Material.Filled.ArrowForward;
    [Parameter] public Color ButtonColor { get; set; } = Color.Primary;
    
    [Parameter] public RenderFragment? FooterContent { get; set; }
    
    [Parameter] public EventCallback OnClick { get; set; }

    private string GetHeaderStyle()
    {
        return $"background-image: linear-gradient(to bottom right, {GradientStart}, {GradientEnd});";
    }

    private string GetLabelStyle()
    {
        var style = "border-radius: 6px; padding: 4px 8px; font-weight: 500; font-size: 0.75rem; backdrop-filter: blur(4px);";
        if (!string.IsNullOrEmpty(LabelBackgroundColor))
        {
            style += $"background-color: {LabelBackgroundColor};";
        }

        if (!string.IsNullOrEmpty(LabelTextColor))
        {
            style += $"color: {LabelTextColor};";
        }

        if (!string.IsNullOrEmpty(LabelBorderColor))
        {
            style += $"border: 1px solid {LabelBorderColor};";
        }
        else 
        {
            // Default transparent border to maintain layout
             style += "border: 1px solid transparent;";
        }
        return style;
    }
}
