using System.Security.Cryptography;
using System.Text;

namespace Freeqy_APIs.Services;

/// <summary>
/// Mock implementation of IPasswordHasher for development.
/// replace this with proper BCrypt/Argon2 implementation.
/// </summary>
public class MockPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Simple SHA256 hash for mock purposes
        // TODO: Replace with BCrypt or Argon2 in production
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }
}
