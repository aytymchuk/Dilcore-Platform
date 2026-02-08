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
        var style = "";
        if (!string.IsNullOrEmpty(LabelBackgroundColor))
        {
            style += $"background-color: {LabelBackgroundColor};";
        }

        if (!string.IsNullOrEmpty(LabelTextColor))
        {
            style += $"color: {LabelTextColor};";
        }
        return style;
    }
}
