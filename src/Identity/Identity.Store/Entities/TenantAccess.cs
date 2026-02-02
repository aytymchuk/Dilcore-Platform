namespace Dilcore.Identity.Store.Entities;

public class TenantAccess
{
    public required string TenantId { get; set; }
    public HashSet<string> Roles { get; set; } = [];
}