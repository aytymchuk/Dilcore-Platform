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
            return UserCreationResult.Failure($"User '{userId}' is already registered.");
        }

        _state.State.Id = Guid.NewGuid();
        _state.State.IdentityId = userId;
        _state.State.Email = email;
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

    private UserResponse ToResponse() => new(
        _state.State.Id,
        _state.State.Email,
        _state.State.FirstName,
        _state.State.LastName,
        _state.State.RegisteredAt);
}
