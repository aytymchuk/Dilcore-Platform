using Dilcore.WebApp.Features.Users.CurrentUser;
using Dilcore.WebApp.Models.Users;
using Dilcore.WebApp.Validation;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Users.Register;

/// <summary>
/// Code-behind for the Register page.
/// </summary>
public partial class Register
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
    private bool _isSubmitting;
    private bool _isFormValid;
    private bool _isLoading = true;

    /// <summary>
    /// FluentValidation wrapper for MudBlazor form validation.
    /// </summary>
    private Func<object, string, Task<IEnumerable<string>>> ValidateValue => _validationAdapter.ValidateValue;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
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

        // Check if user already exists in the system
        var existingUserResult = await Sender.Send(new GetCurrentUserQuery());
        
        if (existingUserResult.IsSuccess && existingUserResult.Value is not null)
        {
            AppNavigator.ToHome(forceLoad: true);
            return;
        }

        _isFormValid = await _validationAdapter.ValidateAsync(_model);
        _isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Allow MudBlazor components to initialize their internal state
            await Task.Delay(500);
            await _form.Validate();
            
            if (_isFormValid != _form.IsValid)
            {
                _isFormValid = _form.IsValid;
                StateHasChanged();
            }
        }
    }

    private async Task OnSubmitAsync()
    {
        await _form.Validate();

        if (!_form.IsValid)
        {
            return;
        }

        _isSubmitting = true;

        try
        {
            var command = new RegisterCommand(_model);
            var result = await Sender.Send(command);

            if (result.IsSuccess)
            {
                Snackbar.Add("Registration successful! Welcome to the platform.", MudBlazor.Severity.Success);
                AppNavigator.ToHome(forceLoad: true);
            }
            // Note: Errors are handled by SnackbarResultBehavior
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
