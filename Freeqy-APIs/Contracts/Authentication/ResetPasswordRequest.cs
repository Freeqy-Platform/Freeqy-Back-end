using System.ComponentModel.DataAnnotations;

namespace Freeqy_APIs.Contracts.Authentication;

public record ResetPasswordRequest(

    string Token,
    string Email,
    string NewPassword
);
