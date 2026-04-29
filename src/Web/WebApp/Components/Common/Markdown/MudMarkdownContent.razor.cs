using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Markdown;

public partial class MudMarkdownContent : ComponentBase
{
    /// <summary>Optional markdown body; empty renders nothing useful but is allowed.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>When null, <see cref="DefaultProps"/> is used (compact headings for agent UI).</summary>
    [Parameter]
    public MudMarkdownProps? Props { get; set; }

    internal static MudMarkdownProps DefaultProps { get; } = new()
    {
        Heading =
        {
            OverrideTypo = typo => typo switch
            {
                Typo.h1 => Typo.h5,
                Typo.h2 => Typo.h6,
                Typo.h3 => Typo.subtitle1,
                _ => Typo.body2,
            },
        },
    };

    private MudMarkdownProps EffectiveProps => Props ?? DefaultProps;
}
