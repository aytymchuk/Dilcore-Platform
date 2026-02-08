using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Components.Common.Dialogs;
using Dilcore.WebApp.Models.Tenants;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;

namespace Dilcore.WebApp.Features.Tenants.List;

public partial class TenantList : AsyncComponentBase
{
    [Inject] public ISender Mediator { get; set; } = default!;
    [Inject] public IDialogService DialogService { get; set; } = default!;

    private List<Tenant>? _tenants;

    protected override async Task OnInitializedAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            var result = await Mediator.Send(new GetTenantListQuery());
            if (result.IsSuccess)
            {
                _tenants = result.Value;
            }
            else
            {
                // Handle error (e.g., show snackbar)
                _tenants = new List<Tenant>();
            }
        });
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions 
        { 
            NoHeader = true,
            BackgroundClass = "backdrop-blur-sm",
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<Features.Tenants.Create.CreateTenantDialog>("", options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data != null)
        {
            // Refresh the list
            await OnInitializedAsync();
        }
    }
}
