using Dilcore.Tenancy.Actors.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Actors;

/// <summary>
/// Orleans grain representing a tenant entity.
/// Grain key is the tenant name (lower kebab-case).
/// </summary>
public sealed class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantState> _state;
    private readonly ILogger<TenantGrain> _logger;
    private readonly TimeProvider _timeProvider;

    public TenantGrain(
        [PersistentState("tenant", "TenantStore")] IPersistentState<TenantState> state,
        ILogger<TenantGrain> logger,
        TimeProvider timeProvider)
    {
        _state = state;
        _logger = logger;
        _timeProvider = timeProvider;
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

    public async Task<TenantCreationResult> CreateAsync(string displayName, string? description)
    {
        var tenantName = this.GetPrimaryKeyString();

        if (_state.State.IsCreated)
        {
            _logger.LogTenantAlreadyExists(tenantName);
            return TenantCreationResult.Failure($"Tenant '{tenantName}' already exists.");
        }

        _state.State.SystemName = tenantName;
        _state.State.Name = displayName;
        _state.State.StoragePrefix = tenantName;
        _state.State.Description = description;
        _state.State.CreatedAt = _timeProvider.GetUtcNow().DateTime;
        _state.State.IsCreated = true;
        _state.State.Id = Guid.NewGuid();

        await _state.WriteStateAsync();

        _logger.LogTenantCreated(tenantName, displayName);

        return TenantCreationResult.Success(ToDto());
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
        _state.State.Id,
        _state.State.Name, // Name (display name)
        _state.State.SystemName,
        _state.State.Description,
        _state.State.CreatedAt);
}
