using System.ComponentModel.DataAnnotations;

namespace Freeqy_APIs.Contracts.Authentication;

public record ResetPasswordRequest
{
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public required string NewPassword { get; init; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; init; }
}
