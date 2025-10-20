using Freeqy_APIs.Entities;

namespace Freeqy_APIs.Repositories;

/// <summary>
/// Repository for managing password reset tokens.
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Creates a new password reset token.
    /// </summary>
    Task<PasswordResetToken> CreateTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a password reset token by its value.
    /// </summary>
    Task<PasswordResetToken?> GetTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a token as used.
    /// </summary>
    Task MarkTokenAsUsedAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired tokens (cleanup operation).
    /// </summary>
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
