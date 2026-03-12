using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Services;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Features.Tenants.Create;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MediatR;

namespace Dilcore.WebApp.Features.Tenants.List;

public partial class TenantList : AsyncComponentBase
{
    private List<Tenant>? _tenants;

    [Inject]
    private ISender Mediator { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IAppNavigator AppNavigator { get; set; } = default!;

    [CascadingParameter]
    public TenantState? CurrentTenantState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTenantsAsync();
    }

    private async Task LoadTenantsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await Mediator.Send(new GetTenantListQuery());

            if (result.IsSuccess)
            {
                _tenants = result.Value;
            }
            else
            {
                // Error handled in Behavior
                _tenants = new List<Tenant>();
            }
        }, LoadingConstants.Tenants);
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions
        {
            NoHeader = true,
            BackgroundClass = "backdrop-blur-sm",
            CloseOnEscapeKey = true,
            // Disable default backdrop click to handle it manually with animation
            BackdropClick = true
        };

        var dialog = await DialogService.ShowAsync<CreateTenantDialog>("", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data != null)
        {
            await OnInitializedAsync();
        }
    }

    private bool IsActive(Tenant tenant)
    {
        return CurrentTenantState?.SystemName == tenant.SystemName;
    }

    private void OnTenantSelected(Tenant tenant)
    {
        AppNavigator.ToTenantWorkspace(tenant.SystemName);
    }
}
