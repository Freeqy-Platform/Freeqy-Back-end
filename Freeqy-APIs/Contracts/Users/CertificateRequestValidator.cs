namespace Freeqy_APIs.Contracts.Users;

public class CertificateRequestValidator : AbstractValidator<CertificateRequest>
{
	public CertificateRequestValidator()
	{
		RuleFor(x => x.CertificateName)
			.NotEmpty().WithMessage("Certificate name is required")
			.MaximumLength(200).WithMessage("Certificate name must not exceed 200 characters");

		RuleFor(x => x.Issuer)
			.MaximumLength(200).WithMessage("Issuer must not exceed 200 characters")
			.When(x => !string.IsNullOrEmpty(x.Issuer));

		RuleFor(x => x.CredentialId)
			.MaximumLength(100).WithMessage("Credential ID must not exceed 100 characters")
			.When(x => !string.IsNullOrEmpty(x.CredentialId));

		RuleFor(x => x.CredentialUrl)
			.MaximumLength(500).WithMessage("Credential URL must not exceed 500 characters")
			.Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
			.WithMessage("Credential URL must be a valid URL")
			.When(x => !string.IsNullOrEmpty(x.CredentialUrl));

		RuleFor(x => x.ExpirationDate)
			.GreaterThanOrEqualTo(x => x.IssueDate)
			.WithMessage("Expiration date must be after or equal to issue date")
			.When(x => x.IssueDate.HasValue && x.ExpirationDate.HasValue);

		RuleFor(x => x.IssueDate)
			.LessThanOrEqualTo(DateTime.UtcNow)
			.WithMessage("Issue date cannot be in the future")
			.When(x => x.IssueDate.HasValue);
	}
}
