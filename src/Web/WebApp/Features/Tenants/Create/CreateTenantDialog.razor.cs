using Dilcore.Tenancy.Contracts.Tenants.Create;
using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Validation;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Features.Tenants.Create;

public partial class CreateTenantDialog : AsyncComponentBase
{
    [Inject] private ITenancyClient TenancyClient { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private ILogger<CreateTenantDialog> Logger { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private MudForm _form = default!;
    private readonly CreateTenantParameters _model = new();
    private readonly FluentValidationAdapter<CreateTenantParameters> _validationAdapter = new(new CreateTenantParametersValidator());
    private bool _isFormValid;

    private Func<object, string, Task<IEnumerable<string>>> ValidateValue => _validationAdapter.ValidateValue;

    private async Task SubmitAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        await ExecuteBusyAsync(async () =>
        {
            try 
            {
                var dto = new CreateTenantDto
                {
                    Name = _model.Name,
                    Description = _model.Description
                };

                var result = await TenancyClient.CreateTenantAsync(dto);
                
                Snackbar.Add("Tenant created successfully", Severity.Success);
                MudDialog.Close(DialogResult.Ok(result));
            }
            catch (Exception ex)
            {
                Logger.LogTenantCreationError(ex);
                Snackbar.Add("Failed to create tenant. Please try again or contact support.", Severity.Error);
            }
        });
    }
}
