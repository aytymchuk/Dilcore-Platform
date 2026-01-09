using Dilcore.Identity.Actors.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Dilcore.Identity.Actors;

/// <summary>
/// Orleans grain representing a user entity.
/// Grain key is the user ID from IUserContext.Id.
/// </summary>
public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserState> _state;
    private readonly ILogger<UserGrain> _logger;

    public UserGrain(
        [PersistentState("user", "UserStore")] IPersistentState<UserState> state,
        ILogger<UserGrain> logger)
    {
        _state = state;
        _logger = logger;
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

    public async Task<UserDto> RegisterAsync(string email, string fullName)
    {
        var userId = this.GetPrimaryKeyString();

        if (_state.State.IsRegistered)
        {
            _logger.LogUserAlreadyRegistered(userId);
            return ToDto();
        }

        _state.State.Id = userId;
        _state.State.Email = email;
        _state.State.FullName = fullName;
        _state.State.RegisteredAt = DateTime.UtcNow;
        _state.State.IsRegistered = true;

        await _state.WriteStateAsync();

        _logger.LogUserRegistered(userId, email);

        return ToDto();
    }

    public Task<UserDto?> GetProfileAsync()
    {
        if (!_state.State.IsRegistered)
        {
            _logger.LogUserNotFound(this.GetPrimaryKeyString());
            return Task.FromResult<UserDto?>(null);
        }

        return Task.FromResult<UserDto?>(ToDto());
    }

    private UserDto ToDto() => new(
        _state.State.Id ?? this.GetPrimaryKeyString(),
        _state.State.Email,
        _state.State.FullName,
        _state.State.RegisteredAt);
}
