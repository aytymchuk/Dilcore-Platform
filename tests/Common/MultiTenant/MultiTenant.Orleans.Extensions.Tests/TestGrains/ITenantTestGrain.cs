namespace Dilcore.MultiTenant.Orleans.Extensions.Tests.TestGrains;

/// <summary>
/// Test grain interface for verifying tenant context propagation.
/// </summary>
public interface ITenantTestGrain : IGrainWithStringKey
{
    /// <summary>
    /// Gets the current tenant name from the injected ITenantContext.
    /// </summary>
    Task<string?> GetCurrentTenantNameAsync();

    /// <summary>
    /// Gets the current tenant storage identifier from the injected ITenantContext.
    /// </summary>
    Task<string?> GetCurrentTenantStorageIdentifierAsync();

    /// <summary>
    /// Gets the current tenant identifier from the injected ITenantContext.
    /// </summary>
    Task<Guid> GetCurrentTenantIdAsync();

    /// <summary>
    /// Calls another grain and returns the tenant name it sees.
    /// Used to test tenant context propagation across grain calls.
    /// </summary>
    Task<string?> CallAnotherGrainAndGetTenantNameAsync(string otherGrainId);
}
