using Dilcore.Domain.Abstractions;
using Dilcore.Domain.Abstractions.Extensions;

namespace Dilcore.Identity.Domain;

public record User : BaseDomain
{
    public required string IdentityId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string FullName => $"{FirstName} {LastName}";
    public List<TenantAccess> Tenants { get; init; } = [];

    public static User Create(string identityId, string email, string firstName, string lastName, TimeProvider timeProvider)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            IdentityId = identityId,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        }.SetCreatedAt(timeProvider)
        .UpdateETag(timeProvider);
    }
}