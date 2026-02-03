namespace Dilcore.Identity.Domain;

public record TenantAccess
{
    public required string TenantId { get; init; }
    public HashSet<string> Roles { get; init; } = [];
}
