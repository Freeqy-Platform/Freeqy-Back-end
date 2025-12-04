namespace Freeqy_APIs.Contracts.Users;

public record CertificateRequest(
	string CertificateName,
	string? Issuer,
	DateTime? IssueDate,
	DateTime? ExpirationDate,
	string? CredentialId,
	string? CredentialUrl,
	string? Description
);
