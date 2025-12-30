namespace Freeqy_APIs.Configrautions;

public class GoogleOAuthOptions
{
    public const string SectionName = "Authentication:Google";
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = [];
}
