namespace Freeqy_APIs.Contracts.Authentication;

public record ConfirmationEmailRequest
(
        string Id,
        string Code
);