using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Features.Tenants.Get;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Services;
using Dilcore.WebApp.Constants;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Features.Tenants.Context;

/// <summary>
/// Cascading state provider for tenant context that resolves tenant from URL and provides it to child components.
/// </summary>
public partial class TenantStateProvider : AsyncComponentBase, IDisposable
{
    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private IBlazorTenantAccessor TenantAccessor { get; set; } = null!;

    [Parameter, EditorRequired]
    public string SystemName { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public TenantState? CurrentTenantState { get; private set; }

    public string? ErrorMessage { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(SystemName))
        {
            return;
        }
        
        await LoadTenantAsync();
    }

    private CancellationTokenSource? _reloadCts;

    private async Task LoadTenantAsync()
    {
        // Cancel previous load if any
        _reloadCts?.Cancel();
        _reloadCts?.Dispose();
        _reloadCts = new CancellationTokenSource();
        var token = _reloadCts.Token;

        try 
        {
            await ExecuteAsync(async () =>
            {
                // Redundant check removed as OnParametersSetAsync handles it
                
                TenantAccessor.TenantName = SystemName;

                var result = await Sender.Send(new GetCurrentTenantQuery(), token);

                if (token.IsCancellationRequested) return;

                if (result.IsFailed)
                {
                    CurrentTenantState = null;
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Unspecified error occurred.";
                    return;
                }

                if (result.ValueOrDefault is null)
                {
                    CurrentTenantState = null;
                    ErrorMessage = "Tenant not found.";
                    return;
                }

                if (!result.Value.SystemName.Equals(SystemName, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = $"Tenant '{SystemName}' not found or you don't have access to it.";
                    CurrentTenantState = null;
                    return;
                }

                CurrentTenantState = new TenantState(result.Value.SystemName, result.Value.Name);
                ErrorMessage = null;
            }, LoadingConstants.WorkspaceData);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
    }

    public void Dispose()
    {
        _reloadCts?.Cancel();
        _reloadCts?.Dispose();
    }
}
