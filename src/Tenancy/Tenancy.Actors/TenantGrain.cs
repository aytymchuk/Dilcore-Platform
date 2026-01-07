using Dilcore.Tenancy.Actors.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Dilcore.Tenancy.Actors;

/// <summary>
/// Orleans grain representing a tenant entity.
/// Grain key is the tenant name (lower kebab-case).
/// </summary>
public sealed class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantState> _state;
    private readonly ILogger<TenantGrain> _logger;

    public TenantGrain(
        [PersistentState("tenant", "TenantStore")] IPersistentState<TenantState> state,
        ILogger<TenantGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogTenantGrainActivating(this.GetPrimaryKeyString());
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogTenantGrainDeactivating(this.GetPrimaryKeyString(), reason.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<TenantDto> CreateAsync(string displayName, string description)
    {
        var tenantName = this.GetPrimaryKeyString();

        if (_state.State.IsCreated)
        {
            _logger.LogTenantAlreadyExists(tenantName);
            return ToDto();
        }

        _state.State.Name = tenantName;
        _state.State.DisplayName = displayName;
        _state.State.Description = description;
        _state.State.CreatedAt = DateTime.UtcNow;
        _state.State.IsCreated = true;

        await _state.WriteStateAsync();

        _logger.LogTenantCreated(tenantName, displayName);

        return ToDto();
    }

    public Task<TenantDto?> GetAsync()
    {
        if (!_state.State.IsCreated)
        {
            _logger.LogTenantNotFound(this.GetPrimaryKeyString());
            return Task.FromResult<TenantDto?>(null);
        }

        return Task.FromResult<TenantDto?>(ToDto());
    }

    private TenantDto ToDto() => new(
        _state.State.Name,
        _state.State.DisplayName,
        _state.State.Description,
        _state.State.CreatedAt);
}
