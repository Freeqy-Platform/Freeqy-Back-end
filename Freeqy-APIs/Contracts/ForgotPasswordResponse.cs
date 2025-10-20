namespace Freeqy_APIs.Contracts;

public record ForgotPasswordResponse
{
    public required string Message { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
