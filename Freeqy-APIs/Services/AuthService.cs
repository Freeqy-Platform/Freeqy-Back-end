using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Freeqy_APIs.Helper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using static Freeqy_APIs.Contracts.Authentication.RegisterRequest;


namespace Freeqy_APIs.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider,
    SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger, IHttpContextAccessor accessor, IEmailSender emailService, IConfiguration configuration) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;
    private readonly IEmailSender _emailService = emailService;
    private readonly string _frontendOrigin = configuration["AppSettings:FrontendOrigin"] ?? "http://localhost:5173";
    private readonly int _refreshTokenExpiryInDays = 15;

    public async Task<Result<AuthResponse>> HandleGoogleLoginAsync()
    {
        return await HandleExternalLoginAsync();
    }

    public async Task<Result<AuthResponse>> HandleGitHubLoginAsync()
    {
        return await HandleExternalLoginAsync();
    }

    private async Task<Result<AuthResponse>> HandleExternalLoginAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidExternalLogin);

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";
        
        // GitHub doesn't provide GivenName/Surname, try to get name and split it
        if (string.IsNullOrEmpty(firstName) && info.LoginProvider == "GitHub")
        {
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "";
            var nameParts = name.Split(' ', 2);
            firstName = nameParts.Length > 0 ? nameParts[0] : "";
            lastName = nameParts.Length > 1 ? nameParts[1] : "";
        }

        if (email == null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidExternalLogin);

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true
        );

        ApplicationUser user;

        if (signInResult.Succeeded)
        {
            user = await _userManager.FindByLoginAsync(
                info.LoginProvider,
                info.ProviderKey
            );
        }
        else
        {
            user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // For GitHub, try to use the GitHub username first
                string username;
                if (info.LoginProvider == "GitHub")
                {
                    var githubUsername = info.Principal.FindFirstValue("urn:github:login") 
                        ?? info.Principal.FindFirstValue("urn:github:name");
                    username = await GenerateUniqueUsernameAsync(email, githubUsername);
                }
                else
                {
                    username = await GenerateUniqueUsernameAsync(email);
                }
                
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user);
            }
            
            await _userManager.AddLoginAsync(user, info);
        }

        await _signInManager.SignInAsync(user, false);
        
        var (token, expiresIn) = _jwtProvider.GenerateToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays);

        user.RefreshTokens.Add(new RefreshToken()
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiryDate
            }
        );
            
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, token, expiresIn, refreshToken, refreshTokenExpiryDate);

        return Result.Success(response);
    }

    
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
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays);

            user.RefreshTokens.Add(new RefreshToken()
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiryDate
                }
            );
            
            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, token, expiresIn, refreshToken, refreshTokenExpiryDate);

            return Result.Success(response);
        }   

        var error = result.IsNotAllowed
            ? UserErrors.EmailNotConfirmed
            : result.IsLockedOut
            ? UserErrors.LockedUser
            : UserErrors.InvalidCredentials;

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken,
        CancellationToken cancellationToken = default)
    {
        string? userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }
        
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        if (user.LockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);
        }
        
        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && !x.IsRevoked);

        if (userRefreshToken is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        if (userRefreshToken.IsExpired)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }
        
        userRefreshToken.RevokedOn = DateTime.UtcNow;
        
        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays);

        user.RefreshTokens.Add(new RefreshToken()
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiryDate
            }
        );
            
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, newToken, expiresIn, newRefreshToken, refreshTokenExpiryDate);

        return Result.Success(response);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken,
        CancellationToken cancellationToken = default)
    {
        string? userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }
        
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        if (user.LockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);
        }
        
        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && !x.IsRevoked);

        if (userRefreshToken is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }
        
        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
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
                { "{{action_url}}", $"{_frontendOrigin}/emailConfirmation?userId={user.Id}&code={code}" }
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
                { "{{action_url}}", $"{_frontendOrigin}/resetPassword?userId={user.Id}&code={code}" }
            }
        );
        
        await _emailService.SendEmailAsync(user.Email!, "Reset Password", emailBody);
        await Task.CompletedTask;
    }
    
    private async Task<string> GenerateUniqueUsernameAsync(string email, string? preferredUsername = null)
    {
        // If a preferred username is provided (e.g., GitHub username), try to use it first
        if (!string.IsNullOrWhiteSpace(preferredUsername))
        {
            var sanitizedPreferred = Regex.Replace(preferredUsername, @"[^a-zA-Z0-9_]", "");
            if (!string.IsNullOrWhiteSpace(sanitizedPreferred))
            {
                var existingUser = await _userManager.FindByNameAsync(sanitizedPreferred);
                if (existingUser == null)
                {
                    return sanitizedPreferred;
                }
                
                // Try with numbers appended
                var counter = 1;
                var candidateUsername = $"{sanitizedPreferred}{counter}";
                while (await _userManager.FindByNameAsync(candidateUsername) != null)
                {
                    counter++;
                    candidateUsername = $"{sanitizedPreferred}{counter}";
                }
                return candidateUsername;
            }
        }
        
        // Fallback: Extract the part before @ from email
        var baseUsername = email.Split('@')[0];
        
        // Remove any invalid characters (keep only letters, numbers, and underscores)
        baseUsername = Regex.Replace(baseUsername, @"[^a-zA-Z0-9_]", "");
        
        // Ensure username is not empty
        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            baseUsername = "user";
        }
        
        // Check if username already exists
        var existing = await _userManager.FindByNameAsync(baseUsername);
        
        if (existing == null)
        {
            return baseUsername;
        }
        
        // If username exists, append a number to make it unique
        var num = 1;
        var candidate = $"{baseUsername}{num}";
        
        while (await _userManager.FindByNameAsync(candidate) != null)
        {
            num++;
            candidate = $"{baseUsername}{num}";
        }
        
        return candidate;
    }
}