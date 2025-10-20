namespace Freeqy_APIs.Services;

/// <summary>
/// Interface for email sending operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, DateTime expiresAt, CancellationToken cancellationToken = default);
}
