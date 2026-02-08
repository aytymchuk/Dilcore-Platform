using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Models.Users;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Features.Users.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dilcore.WebApp.Features.Users;

/// <summary>
/// Cascading state provider for current user information.
/// Loads the user on initialization and provides loading/error state.
/// </summary>
public partial class UserStateProvider : AsyncComponentBase
{
    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the current user, or null if not loaded or not found.
    /// </summary>
    public UserModel? CurrentUser { get; private set; }

    /// <summary>
    /// Gets whether the user was not found (needs registration).
    /// </summary>
    public bool IsUserNotFound { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        // Only load user if authenticated
        if (authState.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        await LoadCurrentUserAsync();
    }

    private async Task LoadCurrentUserAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            var result = await Sender.Send(new GetCurrentUserQuery());

            if (result.IsSuccess && result.Value is not null)
            {
                CurrentUser = result.Value;
                IsUserNotFound = false;
                return;
            }

            if ((result.IsSuccess && result.Value is null) || result.Errors.OfType<UserNotFoundError>().Any())
            {
                IsUserNotFound = true;
                CurrentUser = null;

                // Navigate to registration page and return to suppress the error from snackbar behavior
                NavigationManager.NavigateTo(RouteConstants.Users.Register, forceLoad: false);
                return;
            }
            // Other errors are handled by SnackbarResultBehavior
        });
    }
}