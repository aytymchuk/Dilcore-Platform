using Dilcore.DocumentDb.Abstractions;

namespace Dilcore.Identity.Store.Entities;

/// <summary>
/// MongoDB document entity for persisting User data.
/// </summary>
public class UserDocument : IDocumentEntity
{
    public Guid Id { get; set; }
    public long ETag { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The identity provider's unique identifier for the user (e.g., Auth0 user ID).
    /// </summary>
    public required string IdentityId { get; set; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// The tenants the user has access to.
    /// </summary>
    public List<TenantAccess> Tenants { get; set; } = [];
}