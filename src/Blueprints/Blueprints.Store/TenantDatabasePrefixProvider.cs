using Dilcore.DocumentDb.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Store;

public class TenantDatabasePrefixProvider : IDocumentDatabasePrefixProvider
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantDatabasePrefixProvider(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public Task<Result<string>> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContextResolver.TryResolve(out var tenantContext) || tenantContext is null)
            return Task.FromResult(Result.Fail<string>("Tenant context could not be resolved."));

        if (string.IsNullOrWhiteSpace(tenantContext.StorageIdentifier))
            return Task.FromResult(Result.Fail<string>("Tenant storage identifier is not available."));

        return Task.FromResult(Result.Ok($"{tenantContext.StorageIdentifier}_"));
    }
}
