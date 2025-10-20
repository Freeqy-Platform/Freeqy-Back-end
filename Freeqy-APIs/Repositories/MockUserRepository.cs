using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Repositories;

/// <summary>
/// Mock implementation of IUserRepository for development.
/// This will be replaced by your teammate's actual implementation.
/// </summary>
public class MockUserRepository : IUserRepository
{
    // Simulated user database
    private readonly List<(string Email, string PasswordHash)> _users = new()
    {
        ("test@example.com", "hashed_password_123"), // Sample user for testing
    };

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var userExists = _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        // Return a user if exists, otherwise null
        return Task.FromResult(userExists ? new User() : null);
    }

    public Task UpdateUserPasswordAsync(string email, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user != default)
        {
            _users.Remove(user);
            _users.Add((email, passwordHash));
        }
        return Task.CompletedTask;
    }
}
