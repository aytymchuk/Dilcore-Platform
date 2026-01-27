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
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private MudBlazor.ISnackbar Snackbar { get; set; } = null!;

    private MudBlazor.MudForm _form = null!;
    private readonly RegisterUserParameters _model = new();
    private readonly FluentValidationAdapter<RegisterUserParameters> _validationAdapter = new(new RegisterUserParametersValidator());
    private bool _isSubmitting;

    /// <summary>
    /// FluentValidation wrapper for MudBlazor form validation.
    /// </summary>
    private Func<object, string, Task<IEnumerable<string>>> ValidateValue => _validationAdapter.ValidateValue;

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
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            // Note: Errors are handled by SnackbarResultBehavior
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
