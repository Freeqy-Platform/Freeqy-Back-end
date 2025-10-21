namespace Freeqy_APIs.Contracts.Authentication;

public record ForgetPasswordResponse
{
    public required string Message { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
