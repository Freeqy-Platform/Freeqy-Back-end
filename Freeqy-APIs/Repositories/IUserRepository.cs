using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Repositories;

/// <summary>
/// Interface for user repository operations.
/// This will be implemented by your teammate handling user management.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user's password hash.
    /// </summary>
    Task UpdateUserPasswordAsync(string email, string passwordHash, CancellationToken cancellationToken = default);
}
