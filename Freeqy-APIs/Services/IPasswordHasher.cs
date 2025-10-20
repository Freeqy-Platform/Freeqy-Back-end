namespace Freeqy_APIs.Services;

/// <summary>
/// Interface for password hashing operations.
/// This will be implemented by your teammate handling authentication.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
