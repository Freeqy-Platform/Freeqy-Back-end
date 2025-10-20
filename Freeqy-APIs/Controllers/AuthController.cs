using Freeqy_APIs.Abstractions;
using Freeqy_APIs.Contracts;
using Freeqy_APIs.Services;
using Microsoft.AspNetCore.Mvc;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPasswordResetService passwordResetService,
        ILogger<AuthController> logger)
    {
        _passwordResetService = passwordResetService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates the forgot password flow by sending a reset token to the user's email.
    /// </summary>
    /// <param name="request">The forgot password request containing the user's email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response containing the reset token and expiration time.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Forgot password request received for email: {Email}", request.Email);

        var result = await _passwordResetService.ForgotPasswordAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="request">The reset password request containing the token and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message if the password was reset.</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reset password request received");

        var result = await _passwordResetService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return Ok(result.Value);
    }

    // Adham TODO: add the following endpoints here:
    // - POST /api/auth/register
    // - POST /api/auth/login
    // - POST /api/auth/refresh-token
    // - POST /api/auth/logout
}
