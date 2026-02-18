using Dilcore.WebApp.Features.Users;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class UserMenu
{
    [CascadingParameter]
    public UserStateProvider? UserState { get; set; }
}
