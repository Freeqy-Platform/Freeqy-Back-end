using Microsoft.AspNetCore.WebUtilities;

namespace Freeqy_APIs.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider,
    SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<AuthService> _logger = logger;
    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {  
        if (await _userManager.FindByEmailAsync(email) is not { } user)
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
            return Result.Failure<AuthResponse>(UserErrors.DuplicaetdEmail);  
        
        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);    

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);    
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Email confirmation code: {Code}", code); 

            //TODO: Send email
            return Result.Success();    
        }

        var error = result.Errors.First();

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ForgotPasswordAsync(ForgetPasswordRequest request, CancellationToken cancellationToken = default)
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

        // Send Email
        
        return Result.Success();
    }
    
    

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
            return Result.Success();

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
}