using System.Security.Cryptography;
using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Entities;
using Freeqy_APIs.Errors;
using Freeqy_APIs.Repositories;

namespace Freeqy_APIs.Services;

/// <summary>
/// Service for handling password reset operations.
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<PasswordResetService> _logger;

    private const int TokenExpirationHours = 1;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        ILogger<PasswordResetService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(
        ForgotPasswordRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);

            // Check if user exists
            var user = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Forgot password request for non-existent email: {Email}", request.Email);
                return Result.Failure<ForgotPasswordResponse>(PasswordResetErrors.UserNotFound);
            }

            // Generate secure reset token
            var resetToken = GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours);

            // Create and store the reset token
            var passwordResetToken = new PasswordResetToken
            {
                Email = request.Email,
                Token = resetToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsUsed = false
            };

            await _tokenRepository.CreateTokenAsync(passwordResetToken, cancellationToken);

            // Send password reset email
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                request.Email, 
                resetToken, 
                expiresAt, 
                cancellationToken);

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to: {Email}", request.Email);
                return Result.Failure<ForgotPasswordResponse>(PasswordResetErrors.EmailSendFailed);
            }

            _logger.LogInformation("Password reset token generated successfully for: {Email}", request.Email);

            var response = new ForgotPasswordResponse
            {
                Message = "Password reset instructions have been sent to your email.",
                Token = resetToken, // Include token in response for easy testing
                ExpiresAt = expiresAt
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            return Result.Failure<ForgotPasswordResponse>(new Error(
                "PasswordReset.UnexpectedError",
                "An unexpected error occurred while processing your request.",
                500));
        }
    }

    public async Task<Result<ResetPasswordResponse>> ResetPasswordAsync(
        ResetPasswordRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing password reset request");

            // Retrieve the reset token
            var resetToken = await _tokenRepository.GetTokenAsync(request.Token, cancellationToken);
            if (resetToken == null)
            {
                _logger.LogWarning("Invalid password reset token attempted");
                return Result.Failure<ResetPasswordResponse>(PasswordResetErrors.InvalidToken);
            }

            // Check if token is already used
            if (resetToken.IsUsed)
            {
                _logger.LogWarning("Attempt to use already-used reset token for email: {Email}", resetToken.Email);
                return Result.Failure<ResetPasswordResponse>(PasswordResetErrors.TokenAlreadyUsed);
            }

            // Check if token is expired
            if (resetToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired reset token attempted for email: {Email}", resetToken.Email);
                return Result.Failure<ResetPasswordResponse>(PasswordResetErrors.ExpiredToken);
            }

            // Verify user still exists
            var user = await _userRepository.GetUserByEmailAsync(resetToken.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found during password reset: {Email}", resetToken.Email);
                return Result.Failure<ResetPasswordResponse>(PasswordResetErrors.UserNotFound);
            }

            // Hash the new password
            var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

            // Update user's password
            await _userRepository.UpdateUserPasswordAsync(resetToken.Email, passwordHash, cancellationToken);

            // Mark token as used
            await _tokenRepository.MarkTokenAsUsedAsync(request.Token, cancellationToken);

            _logger.LogInformation("Password reset successfully completed for email: {Email}", resetToken.Email);

            var response = new ResetPasswordResponse
            {
                Message = "Your password has been reset successfully. You can now login with your new password."
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request");
            return Result.Failure<ResetPasswordResponse>(new Error(
                "PasswordReset.UnexpectedError",
                "An unexpected error occurred while resetting your password.",
                500));
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
