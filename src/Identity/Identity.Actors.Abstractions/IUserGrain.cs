namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Represents a user entity in the Orleans actor system.
/// Grain key is the user ID from IUserContext.Id.
/// </summary>
[Alias("Dilcore.Identity.Actors.Abstractions.IUserGrain")]
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Registers a new user with the provided profile information.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <returns>The result of the registration operation.</returns>
    [Alias(nameof(RegisterAsync))]
    Task<UserCreationResult> RegisterAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Gets the user's profile information.
    /// </summary>
    /// <returns>The user profile, or null if not registered.</returns>
    [Alias(nameof(GetProfileAsync))]
    Task<UserResponse?> GetProfileAsync();

    /// <summary>
    /// Adds a tenant to the user's access list.
    /// </summary>
    /// <param name="tenantId">The ID (system name) of the tenant.</param>
    /// <param name="roles">The roles the user has in this tenant.</param>
    /// <returns>A task representing the operation.</returns>
    [Alias(nameof(AddTenantAsync))]
    Task AddTenantAsync(string tenantId, IEnumerable<string> roles);

    /// <summary>
    /// Gets the list of tenants the user has access to.
    /// </summary>
    /// <returns>A list of tenant access records.</returns>
    [Alias(nameof(GetTenantsAsync))]
    Task<IReadOnlyList<TenantAccess>> GetTenantsAsync();

    /// <summary>
    /// Checks if the user is registered.
    /// </summary>
    /// <returns>True if the user is registered, false otherwise.</returns>
    [Alias(nameof(IsRegisteredAsync))]
    Task<bool> IsRegisteredAsync();

    /// <summary>
    /// Gets the roles the user has in a specific tenant.
    /// </summary>
    /// <param name="tenantId">The ID (system name) of the tenant.</param>
    /// <returns>A list of roles, or empty list if no access or not registered.</returns>
    [Alias(nameof(GetTenantRolesAsync))]
    Task<IReadOnlyList<string>> GetTenantRolesAsync(string tenantId);
}
