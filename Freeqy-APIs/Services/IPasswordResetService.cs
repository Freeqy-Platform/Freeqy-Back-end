using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts;

namespace Freeqy_APIs.Services;

/// <summary>
/// Interface for password reset operations.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Initiates the forgot password flow by generating a reset token and sending an email.
    /// </summary>
    Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
