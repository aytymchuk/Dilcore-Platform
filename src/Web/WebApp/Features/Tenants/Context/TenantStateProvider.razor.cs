using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Features.Tenants.Get;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Services.Tenancy;
using Dilcore.WebApp.Constants;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Context;

/// <summary>
/// Cascading state provider for tenant context that resolves tenant from URL and provides it to child components.
/// </summary>
public partial class TenantStateProvider : AsyncComponentBase
{
    private string? _currentSystemName;
    private CancellationTokenSource? _reloadCts;

    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private IBlazorTenantContext TenantContext { get; set; } = null!;

    [Parameter, EditorRequired]
    public string SystemName { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public TenantState? CurrentTenantState { get; private set; }

    public string? ErrorMessage { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(SystemName) || SystemName == _currentSystemName)
        {
            return;
        }

        _currentSystemName = SystemName;
        TenantContext.Set(SystemName);

        await LoadTenantAsync();
    }

    public override ValueTask DisposeAsync()
    {
        _reloadCts?.Cancel();
        _reloadCts?.Dispose();

        return base.DisposeAsync();
    }

    private async Task LoadTenantAsync()
    {
        try
        {
            await ExecuteAsync(async () =>
            {
                var result = await Sender.Send(new GetCurrentTenantQuery());

                HandleQueryResult(result);
            }, LoadingConstants.WorkspaceData);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
    }

    private void ResetCts()
    {
        _reloadCts?.Cancel();
        _reloadCts?.Dispose();
        _reloadCts = new CancellationTokenSource();
    }

    private void HandleQueryResult(Result<Tenant> result)
    {
        if (result.IsFailed)
        {
            CurrentTenantState = null;
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Unspecified error occurred.";
            return;
        }

        var tenant = result.ValueOrDefault;
        if (tenant is null)
        {
            CurrentTenantState = null;
            ErrorMessage = "Tenant not found.";
            return;
        }

        if (!tenant.SystemName.Equals(SystemName, StringComparison.OrdinalIgnoreCase))
        {
            CurrentTenantState = null;
            ErrorMessage = $"Tenant '{SystemName}' not found or you don't have access to it.";
            return;
        }

        CurrentTenantState = new TenantState(tenant.SystemName, tenant.Name);
        ErrorMessage = null;
    }
}
