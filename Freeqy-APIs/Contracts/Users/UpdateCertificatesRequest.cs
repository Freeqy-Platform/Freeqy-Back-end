namespace Freeqy_APIs.Contracts.Users;

public record UpdateCertificatesRequest(
	IEnumerable<CertificateRequest> Certificates
);
