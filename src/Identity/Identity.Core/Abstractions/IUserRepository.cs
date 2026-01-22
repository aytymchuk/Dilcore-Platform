using Dilcore.Identity.Domain;
using FluentResults;

namespace Dilcore.Identity.Core.Abstractions;

/// <summary>
/// Repository interface for User operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their identity ID.
    /// </summary>
    Task<Result<User?>> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<Result<User?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a user.
    /// </summary>
    Task<Result<User>> StoreAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by their identity ID.
    /// </summary>
    Task<Result<bool>> DeleteByIdentityIdAsync(string identityId, long eTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id, long eTag, CancellationToken cancellationToken = default);
}