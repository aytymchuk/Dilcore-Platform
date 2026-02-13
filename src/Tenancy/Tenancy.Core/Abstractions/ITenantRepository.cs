using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Abstractions;

public interface ITenantRepository
{
    Task<Result<Tenant>> StoreAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Result<Tenant?>> GetBySystemNameAsync(string systemName, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Tenant>>> GetBySystemNamesAsync(IEnumerable<string> systemNames, CancellationToken cancellationToken = default);
}
