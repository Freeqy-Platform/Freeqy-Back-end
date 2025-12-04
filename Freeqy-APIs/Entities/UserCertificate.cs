namespace Freeqy_APIs.Entities;

public class UserCertificate
{
	public long Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string CertificateName { get; set; } = string.Empty;
	public string? Issuer { get; set; }
	public DateTime? IssueDate { get; set; }
	public DateTime? ExpirationDate { get; set; }
	public string? CredentialId { get; set; }
	public string? CredentialUrl { get; set; }
	public string? Description { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }

	public ApplicationUser User { get; set; } = default!;
}
