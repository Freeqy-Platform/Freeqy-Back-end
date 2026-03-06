using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.RateLimiting;

namespace Freeqy_APIs.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("authentication")]
public class AuthController(ILogger<AuthController> logger,
    IAuthService authService, SignInManager<ApplicationUser> signInManager, IConfiguration configuration) : ControllerBase
{
    
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly SignInManager<ApplicationUser> _signInManager =  signInManager;
    private readonly string _frontendOrigin = configuration["AppSettings:FrontendOrigin"] ?? "http://localhost:5173";
    
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            Url.Action(nameof(GoogleResponse))
        );

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    
    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await _authService.HandleGoogleLoginAsync();
        return HandleOAuthCallback(result);
    }

    [HttpGet("github-login")]
    public IActionResult GitHubLogin()
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "GitHub",
            Url.Action(nameof(GitHubResponse))
        );

        return Challenge(properties, "GitHub");
    }
    
    [HttpGet("github-response")]
    public async Task<IActionResult> GitHubResponse()
    {
        var result = await _authService.HandleGitHubLoginAsync();
        return HandleOAuthCallback(result);
    }

    private IActionResult HandleOAuthCallback(Result<AuthResponse> result)
    {
        if (result.IsSuccess)
        {
            var auth = result.Value;
            var redirectUrl = $"{_frontendOrigin}/oauth/callback" +
                $"?token={Uri.EscapeDataString(auth.Token)}" +
                $"&refreshToken={Uri.EscapeDataString(auth.RefreshToken)}" +
                $"&expiresIn={auth.ExpiresIn}";
            return Redirect(redirectUrl);
        }

        return Redirect($"{_frontendOrigin}/oauth/callback?error=authentication_failed");
    }


    
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

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}