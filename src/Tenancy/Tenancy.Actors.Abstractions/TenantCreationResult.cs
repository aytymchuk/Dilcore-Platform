namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Result of a tenant creation operation.
/// Designed to be Orleans-serializable.
/// </summary>
[GenerateSerializer]
public sealed record TenantCreationResult
{
    [Id(0)]
    public bool IsSuccess { get; init; }

    [Id(1)]
    public TenantDto? Tenant { get; init; }

    [Id(2)]
    public string? ErrorMessage { get; init; }

    public static TenantCreationResult Success(TenantDto tenant) => new() { IsSuccess = true, Tenant = tenant };
    public static TenantCreationResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
