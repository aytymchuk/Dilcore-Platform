using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class AgentMarkdown : ComponentBase
{
    [Parameter, EditorRequired]
    public string? Value { get; set; }

    private static readonly MudMarkdownProps _props = new()
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
}
