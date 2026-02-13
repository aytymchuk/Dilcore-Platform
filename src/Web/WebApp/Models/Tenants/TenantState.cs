namespace Dilcore.WebApp.Models.Tenants;

/// <summary>
/// Represents the current tenant context for cascading to child components.
/// </summary>
public record TenantState(string SystemName, string Name);
