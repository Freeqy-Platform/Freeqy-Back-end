using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("authentication")]
public class AuthController(ILogger<AuthController> logger,
    IAuthService authService) : ControllerBase
{
    
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgetPasswordRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Forgot password request received for email: {Email}", request.Email);

        var result = await _authService.ForgetPasswordAsync(request);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return Ok();
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reset password request received");

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.EmailOrUsername, request.Password, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("resend-confirmation-code")]
    public async Task<IActionResult> ResendConfirmationCode(ResendConfirmationEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ResendConfirmationCodeAsync(request);
        
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmationEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(request);
        
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
    
    
}