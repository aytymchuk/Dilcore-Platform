using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dilcore.Identity.Actors;

/// <summary>
/// Orleans grain representing a user entity.
/// Grain key is the user ID from IUserContext.Id.
/// </summary>
public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserState> _state;
    private readonly ILogger<UserGrain> _logger;
    private readonly TimeProvider _timeProvider;

    public UserGrain(
        [PersistentState("user", "UserStore")] IPersistentState<UserState> state,
        ILogger<UserGrain> logger,
        TimeProvider timeProvider)
    {
        _state = state;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogUserGrainActivating(this.GetPrimaryKeyString());
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogUserGrainDeactivating(this.GetPrimaryKeyString(), reason.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<UserCreationResult> RegisterAsync(string email, string firstName, string lastName)
    {
        var userId = this.GetPrimaryKeyString();

        if (_state.State.IsRegistered)
        {
            _logger.LogUserAlreadyRegistered(userId);
            return UserCreationResult.Failure("User is already registered.");
        }

        _state.State.Id = Guid.CreateVersion7();
        _state.State.IdentityId = userId;
        _state.State.Email = email.ToLowerInvariant();
        _state.State.FirstName = firstName;
        _state.State.LastName = lastName;
        _state.State.RegisteredAt = _timeProvider.GetUtcNow().DateTime;
        _state.State.IsRegistered = true;

        await _state.WriteStateAsync();

        _logger.LogUserRegistered(userId, email);

        return UserCreationResult.Success(ToResponse());
    }

    public Task<UserResponse?> GetProfileAsync()
    {
        if (!_state.State.IsRegistered)
        {
            _logger.LogUserNotFound(this.GetPrimaryKeyString());
            return Task.FromResult<UserResponse?>(null);
        }

        return Task.FromResult<UserResponse?>(ToResponse());
    }


    public async Task AssignTenantOwnerAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or whitespace.", nameof(tenantId));
        }

        // We explicitly skip IsRegistered check here to allow owners to be assigned during registration flow

        var roles = new HashSet<string> { Roles.Owner };
        var existingAccess = _state.State.Tenants.Find(t => t.TenantId == tenantId);

        if (existingAccess is null)
        {
            existingAccess = new TenantAccess { TenantId = tenantId, Roles = roles };
            _state.State.Tenants.Add(existingAccess);
        }
        else
        {
            existingAccess.Roles.Add(Roles.Owner);
        }

        await _state.WriteStateAsync();

        _logger.LogUserTenantOwnerAssigned(this.GetPrimaryKeyString(), tenantId);
    }

    public Task<IReadOnlyList<TenantAccess>> GetTenantsAsync()
    {
        return Task.FromResult<IReadOnlyList<TenantAccess>>(_state.State.Tenants.ToList());
    }

    public Task<bool> IsRegisteredAsync()
    {
        return Task.FromResult(_state.State.IsRegistered);
    }

    public Task<IReadOnlyList<string>> GetTenantRolesAsync(string tenantId)
    {
        var tenantAccess = _state.State.Tenants.Find(t => t.TenantId == tenantId);
        var roles = tenantAccess?.Roles.ToList() ?? [];

        return Task.FromResult<IReadOnlyList<string>>(roles);
    }

    private UserResponse ToResponse() => new(
        _state.State.Id,
        _state.State.Email,
        _state.State.FirstName,
        _state.State.LastName,
        _state.State.RegisteredAt);
}
