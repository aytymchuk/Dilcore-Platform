using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Cards;

public partial class EntityCard
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Subtitle { get; set; } = string.Empty;

    [Parameter]
    public string Description { get; set; } = string.Empty;

    [Parameter]
    public string Role { get; set; } = string.Empty;

    [Parameter]
    public string StatusLabel { get; set; } = string.Empty;

    [Parameter]
    public string ButtonText { get; set; } = string.Empty;

    [Parameter]
    public string ButtonIcon { get; set; } = string.Empty;

    [Parameter]
    public Color ButtonColor { get; set; } = Color.Primary;

    [Parameter]
    public EventCallback OnClick { get; set; }

    private string GetInitials(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0][..2].ToUpper()
                : parts[0].ToUpper();
        }

        return $"{parts[0][0]}{parts[1][0]}".ToUpper();
    }
}