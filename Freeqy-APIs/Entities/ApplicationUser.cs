namespace Freeqy_APIs.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public IEnumerable<RefreshToken> RefreshTokens { get; set; } = [];
}