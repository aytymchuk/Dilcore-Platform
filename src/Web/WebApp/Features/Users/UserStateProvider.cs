using Dilcore.Identity.Contracts.Profile;
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
public partial class UserStateProvider : ComponentBase
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
    /// Gets whether the user data is currently loading.
    /// </summary>
    public bool IsLoading { get; private set; } = true;

    /// <summary>
    /// Gets the current user, or null if not loaded or not found.
    /// </summary>
    public UserDto? CurrentUser { get; private set; }

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
            IsLoading = false;
            return;
        }

        await LoadCurrentUserAsync();
    }

    private async Task LoadCurrentUserAsync()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            var result = await Sender.Send(new GetCurrentUserQuery());

            if (result.IsSuccess)
            {
                CurrentUser = result.Value;
                IsUserNotFound = false;
            }
            else if (result.Errors.OfType<UserNotFoundError>().Any())
            {
                IsUserNotFound = true;
                CurrentUser = null;

                // Navigate to registration page
                NavigationManager.NavigateTo(RouteConstants.Users.Register, forceLoad: false);
            }
            // Other errors are handled by SnackbarResultBehavior
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Refreshes the current user state.
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadCurrentUserAsync();
    }
}
