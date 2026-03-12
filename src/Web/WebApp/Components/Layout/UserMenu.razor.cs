using Dilcore.WebApp.Features.Users;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class UserMenu
{
    [CascadingParameter]
    public UserStateProvider? UserState { get; set; }

    /// <summary>
    /// When true, renders a compact block (avatar + name + subtitle) for use at the bottom of the sidebar.
    /// </summary>
    [Parameter]
    public bool InSidebar { get; set; }

    /// <summary>
    /// Optional subtitle shown in sidebar mode (e.g. "Admin Workspace").
    /// </summary>
    [Parameter]
    public string? Subtitle { get; set; }
}
