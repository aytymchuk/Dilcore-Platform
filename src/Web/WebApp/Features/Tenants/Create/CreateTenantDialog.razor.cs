using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Validation;
using Dilcore.WebApp.Extensions;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Features.Tenants.Create;

public partial class CreateTenantDialog : AsyncComponentBase
{
    private readonly CreateTenantParameters _model = new();
    private readonly FluentValidationAdapter<CreateTenantParameters> _validationAdapter = new(new CreateTenantParametersValidator());

    private MudForm _form = default!;
    private bool _isFormValid;

    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private ILogger<CreateTenantDialog> Logger { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private Func<object, string, Task<IEnumerable<string>>> ValidateValue => _validationAdapter.ValidateValue;

    private async Task SubmitAsync()
    {
        await _form.Validate();

        if (!_form.IsValid)
        {
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            try
            {
                await HandleSubmitAsync();
            }
            catch (Exception ex)
            {
                Logger.LogTenantCreationError(ex);
                Snackbar.Add("Failed to create tenant. Please try again or contact support.", Severity.Error);
            }
        });
    }

    private async Task HandleSubmitAsync()
    {
        var result = await Sender.Send(new CreateTenantCommand(_model));

        if (result.IsSuccess)
        {
            Snackbar.Add("Tenant created successfully", Severity.Success);
            MudDialog.Close(DialogResult.Ok(result.Value));
        }
    }
}
