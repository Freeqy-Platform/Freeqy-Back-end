namespace Freeqy_APIs.Contracts.Users;

public class UpdateSummaryRequestValidator : AbstractValidator<UpdateSummaryRequest>
{
	public UpdateSummaryRequestValidator()
	{
		RuleFor(x => x.Summary)
			.MaximumLength(500).WithMessage("Summary must not exceed 500 characters")
			.When(x => !string.IsNullOrEmpty(x.Summary));
	}
}
