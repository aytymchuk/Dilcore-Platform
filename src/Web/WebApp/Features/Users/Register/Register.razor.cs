using Dilcore.WebApp.Models.Users;
using Dilcore.WebApp.Validation;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Users.Register;

/// <summary>
/// Code-behind for the Register page.
/// </summary>
public partial class Register : AsyncComponentBase
{
    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private Services.IAppNavigator AppNavigator { get; set; } = null!;

    [Inject]
    private MudBlazor.ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private MudBlazor.MudForm _form = null!;
    private readonly RegisterUserParameters _model = new();
    private readonly FluentValidationAdapter<RegisterUserParameters> _validationAdapter = new(new RegisterUserParametersValidator());
    private bool _isFormValid;

    /// <summary>
    /// FluentValidation wrapper for MudBlazor form validation.
    /// </summary>
    private Func<object, string, Task<IEnumerable<string>>> ValidateValue => _validationAdapter.ValidateValue;

    protected override async Task OnInitializedAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            if (await CheckExistingUserAndRedirectAsync())
            {
                return;
            }

            await PopulateModelFromClaimsAsync();

            _isFormValid = await _validationAdapter.ValidateAsync(_model);
        });
    }

    /// <summary>
    /// If the current user already exists in the system, redirects to home and returns true; otherwise returns false.
    /// </summary>
    private async Task<bool> CheckExistingUserAndRedirectAsync()
    {
        var result = await IdentityClient.SafeGetCurrentUserAsync();
        if (result.IsSuccess)
        {
            AppNavigator.ToHome(forceLoad: true);
            return true;
        }

        return false;
    }

    private async Task PopulateModelFromClaimsAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return;
        }

        _model.Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                       ?? user.FindFirst("email")?.Value
                       ?? string.Empty;

        _model.FirstName = user.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value
                           ?? user.FindFirst("given_name")?.Value
                           ?? string.Empty;

        _model.LastName = user.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
                          ?? user.FindFirst("family_name")?.Value
                          ?? string.Empty;
    }

    private async Task OnSubmitAsync()
    {
        await _form.Validate();

        if (!_form.IsValid)
        {
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            var command = new RegisterCommand(_model);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                Snackbar.Add("Registration successful! Welcome to the platform.", MudBlazor.Severity.Success);
                AppNavigator.ToHome(forceLoad: true);
            }
            // Note: Errors are handled by SnackbarResultBehavior
        });
    }
}
