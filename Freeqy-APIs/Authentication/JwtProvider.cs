namespace Freeqy_APIs.Authentication;

public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public (string token, int expiresIn) GenerateToken(ApplicationUser user)
    {
        Claim[] claims = [
            new (JwtRegisteredClaimNames.Sub, user.Id),    
            new (JwtRegisteredClaimNames.Email, user.Email!),    
            new (JwtRegisteredClaimNames.GivenName, user.FirstName),    
            new (JwtRegisteredClaimNames.FamilyName, user.LastName),    
            new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),    
        ];

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var expiresIn = 30;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: signingCredentials
        );

        return (token : new JwtSecurityTokenHandler().WriteToken(token),expiresIn: expiresIn);
    }
}