using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class AgentMarkdown : ComponentBase
{
    [Parameter, EditorRequired]
    public string? Value { get; set; }
}
