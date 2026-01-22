using AutoMapper;
using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Dilcore.Identity.Actors.Storage;

/// <summary>
/// Orleans grain storage provider that persists UserGrain state via IUserRepository.
/// </summary>
public sealed class UserGrainStorage : IGrainStorage
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<UserGrainStorage> _logger;

    public UserGrainStorage(
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ILogger<UserGrainStorage> logger)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var identityId = grainId.Key.ToString()!;
        _logger.LogReadingUserState(identityId);

        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var result = await userRepository.GetByIdentityIdAsync(identityId);

        if (result.IsFailed)
        {
            _logger.LogReadStateError(null, identityId);
            throw new InvalidOperationException($"Failed to read state for user '{identityId}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        if (result.Value is null)
        {
            _logger.LogUserNotFoundForRead(identityId);
            grainState.State = Activator.CreateInstance<T>();
            grainState.RecordExists = false;
            grainState.ETag = null;
            return;
        }

        grainState.State = _mapper.Map<T>(result.Value);
        grainState.RecordExists = true;
        grainState.ETag = result.Value.ETag.ToString();

        _logger.LogUserStateLoaded(identityId);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var identityId = grainId.Key.ToString()!;
        _logger.LogWritingUserState(identityId);

        if (grainState.State is not UserState userState)
        {
            throw new InvalidOperationException($"Expected UserState but got {typeof(T).Name}");
        }

        // Map UserState to User domain entity
        var user = _mapper.Map<User>(userState);

        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var result = await userRepository.StoreAsync(user);

        if (result.IsFailed)
        {
            _logger.LogWriteStateError(null, identityId);
            throw new InvalidOperationException($"Failed to write state for user '{identityId}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        grainState.RecordExists = true;
        grainState.ETag = result.Value.ETag.ToString();

        _logger.LogUserStateWritten(identityId);
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var identityId = grainId.Key.ToString()!;
        _logger.LogClearingUserState(identityId);

        if (grainState.State is not UserState userState)
        {
            throw new InvalidOperationException($"Expected UserState but got {typeof(T).Name}");
        }

        if (string.IsNullOrEmpty(userState.IdentityId))
        {
            _logger.LogInvalidUserId(identityId);
            return;
        }

        var eTag = long.TryParse(grainState.ETag, out var parsedETag) ? parsedETag : 0;

        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var result = await userRepository.DeleteByIdentityIdAsync(userState.IdentityId, eTag);

        if (result.IsFailed)
        {
            _logger.LogClearStateError(null, identityId);
            throw new InvalidOperationException($"Failed to clear state for user '{identityId}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        if (result.Value)
        {
            grainState.State = Activator.CreateInstance<T>();
            grainState.RecordExists = false;
            grainState.ETag = null;
        }

        _logger.LogUserStateCleared(identityId);
    }
}
