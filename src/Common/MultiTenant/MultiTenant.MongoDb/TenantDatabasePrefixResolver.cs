using Dilcore.MongoDB.Abstractions.Namespace;
using Dilcore.MultiTenant.Abstractions;
using FluentResults;

namespace Dilcore.MultiTenant.MongoDb;

/// <summary>
/// Resolves the current tenant storage identifier as a MongoDB database name prefix.
/// Fails when the tenant is missing or has no storage identifier.
/// </summary>
public sealed class TenantDatabasePrefixResolver : INamespacePrefixResolver
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantDatabasePrefixResolver(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public Task<Result<string?>> ResolveAsync(
        NamespaceResolutionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContextResolver.TryResolve(out var tenantContext) || tenantContext is null)
        {
            return Task.FromResult(Result.Fail<string?>("Tenant context could not be resolved."));
        }

        if (string.IsNullOrWhiteSpace(tenantContext.StorageIdentifier))
        {
            return Task.FromResult(Result.Fail<string?>("Tenant storage identifier is not available."));
        }

        return Task.FromResult(Result.Ok<string?>(tenantContext.StorageIdentifier));
    }
}
