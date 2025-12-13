using Freeqy_APIs.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using static Freeqy_APIs.Contracts.Authentication.RegisterRequest;


namespace Freeqy_APIs.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider,
    SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger, IHttpContextAccessor accessor, IEmailSender emailService) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;
    private readonly IEmailSender _emailService = emailService;
    private readonly string _tempOrigin = "http://localhost:5173";
    

    public async Task<Result<AuthResponse>> GetTokenAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = null;

        // Try to find user by email first
        if (emailOrUsername.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(emailOrUsername);
        }
        
        // If not found by email or doesn't contain @, try username
        if (user is null)
        {
            user = await _userManager.FindByNameAsync(emailOrUsername);
        }

        // If user still not found, return invalid credentials
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (result.Succeeded)
        {
            var (token, expiresIn) = _jwtProvider.GenerateToken(user);

            var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, token, expiresIn);

            return Result.Success(response);
        }   

        var error = result.IsNotAllowed
            ? UserErrors.EmailNotConfirmed
            : result.IsLockedOut
            ? UserErrors.LockedUser
            : UserErrors.InvalidCredentials;

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);  
        
        if (emailIsExists)
            return Result.Failure<AuthResponse>(UserErrors.DuplicateEmail);  
        
        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);    

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);    
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Email confirmation code: {Code}", code);

            await SendConfirmationEmail(user, code);
            return Result.Success();    
        }

        var error = result.Errors.First();

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ForgetPasswordAsync(ForgetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success();

        if (!user.EmailConfirmed)
        {
            return Result.Failure(UserErrors.EmailNotConfirmed);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        
        _logger.LogInformation("this is the token {}", token);

        await SendResetPasswordCode(user, token);
        
        return Result.Success();
    }

    public async Task<Result> ResendConfirmationCodeAsync(ResendConfirmationEmailRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
        {
            return Result.Success();
        }

        if (user.EmailConfirmed)
        {
            return  Result.Failure(UserErrors.DuplicateEmailConfirmed);
        }
        
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);    
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Email confirmation code: {Code}", code); 

        await SendConfirmationEmail(user,  code);
        return Result.Success();    
        
    }
    public async Task<Result> ConfirmEmailAsync(ConfirmationEmailRequest request)
    {

        if ( await _userManager.FindByIdAsync(request.Id) is not {} user)
            return Result.Failure(UserErrors.InvalidToken);

        if (user.EmailConfirmed)
        {
            return Result.Failure(UserErrors.DuplicateEmailConfirmed);
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            result = await _userManager.ConfirmEmailAsync(user, code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email");
            return Result.Failure(UserErrors.InvalidToken);
        }
        

        if (result.Succeeded)
        {
            return Result.Success();
        }
        
        var error = result.Errors.First();
        
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id);

        if (user is not { EmailConfirmed: true })
            return Result.Failure(UserErrors.InvalidToken);

        IdentityResult result;
        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        }catch(FormatException ex)
        {
            _logger.LogError(ex, "Error resetting password");
            result  = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
        {
            return  Result.Success();
        }
        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }


    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBuilder.GenerateEmailBody("confirmation-email",
            emailBody: new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{_tempOrigin}/emailConfirmation?userId={user.Id}&code={code}" }
            }
        );
        
        await _emailService.SendEmailAsync(user.Email!, "Freeqy Email Confirmation", emailBody);
        await Task.CompletedTask;
    }


    private async Task SendResetPasswordCode(ApplicationUser user, string code)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
        var emailBody = EmailBuilder.GenerateEmailBody("reset-password",
            new Dictionary<string, string>
            {
                {"{{name}}", user.FirstName },
                { "{{action_url}}", $"{_tempOrigin}/resetPassword?userId={user.Id}&code={code}" }
            }
        );
        
        await _emailService.SendEmailAsync(user.Email!, "Reset Password", emailBody);
        await Task.CompletedTask;
    }
    
    
}