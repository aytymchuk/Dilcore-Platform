using Dilcore.MultiTenant.Abstractions;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests.TestGrains;

/// <summary>
/// Test grain implementation for verifying tenant context propagation.
/// </summary>
[GrainType("dilcore-tenant-test-grain")]
public class TenantTestGrain : Grain, ITenantTestGrain
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantTestGrain(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public Task<string?> GetCurrentTenantNameAsync()
    {
        if (_tenantContextResolver.TryResolve(out var tenantContext))
        {
            return Task.FromResult(tenantContext?.Name);
        }

        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetCurrentTenantStorageIdentifierAsync()
    {
        if (_tenantContextResolver.TryResolve(out var tenantContext))
        {
            return Task.FromResult(tenantContext?.StorageIdentifier);
        }

        return Task.FromResult<string?>(null);
    }

    public Task<Guid> GetCurrentTenantIdAsync()
    {
        if (_tenantContextResolver.TryResolve(out var tenantContext))
        {
            return Task.FromResult(tenantContext?.Id ?? Guid.Empty);
        }

        return Task.FromResult(Guid.Empty);
    }

    public async Task<string?> CallAnotherGrainAndGetTenantNameAsync(string otherGrainId)
    {
        var otherGrain = GrainFactory.GetGrain<ITenantTestGrain>(otherGrainId);
        return await otherGrain.GetCurrentTenantNameAsync();
    }
}
