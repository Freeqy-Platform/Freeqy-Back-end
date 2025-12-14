namespace Freeqy_APIs.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result> ForgetPasswordAsync(ForgetPasswordRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResendConfirmationCodeAsync(ResendConfirmationEmailRequest request);
    Task<Result> ConfirmEmailAsync(ConfirmationEmailRequest request);
}