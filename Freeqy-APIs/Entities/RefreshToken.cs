namespace Freeqy_APIs.Entities;


[Owned]
public class RefreshToken
{
    public string Token { get; set; } =  string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } =  DateTime.Now;
    public DateTime? RevokedOn { get; set; }
    
    public bool IsExpired => ExpiresOn > DateTime.Now;
    public bool IsRevoked => RevokedOn != null && IsExpired;
}
