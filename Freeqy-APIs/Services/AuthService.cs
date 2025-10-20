namespace Freeqy_APIs.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {  
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return null;

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);  

        if (!isValidPassword)
            return null;

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        return new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, token, expiresIn);
    }
}