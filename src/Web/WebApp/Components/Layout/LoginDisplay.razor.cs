using Dilcore.WebApp.Features.Users;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Layout;

public partial class LoginDisplay
{
    [CascadingParameter]
    public UserStateProvider UserState { get; set; } = null!;
}