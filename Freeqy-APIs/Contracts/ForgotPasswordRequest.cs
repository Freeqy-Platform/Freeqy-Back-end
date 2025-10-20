using System.ComponentModel.DataAnnotations;

namespace Freeqy_APIs.Contracts;

public record ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; init; }
}
