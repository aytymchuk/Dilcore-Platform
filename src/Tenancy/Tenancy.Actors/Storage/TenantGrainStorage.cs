using AutoMapper;
using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Tenancy.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Dilcore.Tenancy.Actors.Storage;

public sealed class TenantGrainStorage : IGrainStorage
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<TenantGrainStorage> _logger;

    public TenantGrainStorage(
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ILogger<TenantGrainStorage> logger)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        // Tenant storage keys are just the tenant system name (string key of the grain)
        var systemName = grainId.Key.ToString()!;
        _logger.LogReadingTenantState(systemName);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        var result = await repository.GetBySystemNameAsync(systemName);

        if (result.IsFailed)
        {
            _logger.LogReadStateError(null, systemName);
            throw new InvalidOperationException($"Failed to read state for tenant '{systemName}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        if (result.Value is null)
        {
            _logger.LogTenantNotFoundForRead(systemName);
            grainState.State = Activator.CreateInstance<T>();
            grainState.RecordExists = false;
            grainState.ETag = null;
            return;
        }

        grainState.State = _mapper.Map<T>(result.Value);
        grainState.RecordExists = true;
        grainState.ETag = result.Value.ETag.ToString();

        _logger.LogTenantStateLoaded(systemName);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var systemName = grainId.Key.ToString()!;
        _logger.LogWritingTenantState(systemName);

        if (grainState.State is not TenantState tenantState)
        {
            throw new InvalidOperationException($"Expected TenantState but got {typeof(T).Name}");
        }

        // Map TenantState to Tenant domain entity
        var tenant = _mapper.Map<Tenant>(tenantState);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        var result = await repository.StoreAsync(tenant);

        if (result.IsFailed)
        {
            _logger.LogWriteStateError(null, systemName);
            throw new InvalidOperationException($"Failed to write state for tenant '{systemName}': {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        // Update grain state with the persisted entity (e.g. usage of new ETag/Id)
        grainState.State = _mapper.Map<T>(result.Value);
        grainState.RecordExists = true;
        grainState.ETag = result.Value.ETag.ToString();

        _logger.LogTenantStateWritten(systemName);
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        // Deletion not required/supported for now
        var systemName = grainId.Key.ToString()!;
        _logger.LogClearingTenantState(systemName);
        return Task.CompletedTask;
    }
}
