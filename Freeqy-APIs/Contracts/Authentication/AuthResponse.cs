namespace Freeqy_APIs.Contracts.Authentication;

public record AuthResponse(
    string Id,
    string FirstName,
    string LastName,
    string? Email,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiryDate
);