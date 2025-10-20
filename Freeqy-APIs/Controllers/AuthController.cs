namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IPasswordResetService passwordResetService, ILogger<AuthController> logger,
    IAuthService authService) : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService = passwordResetService;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;

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

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult is null ? BadRequest("Invalid email/password") : Ok(authResult);
    }
}
