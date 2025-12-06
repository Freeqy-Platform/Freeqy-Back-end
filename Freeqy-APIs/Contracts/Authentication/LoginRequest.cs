namespace Freeqy_APIs.Contracts.Authentication;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);