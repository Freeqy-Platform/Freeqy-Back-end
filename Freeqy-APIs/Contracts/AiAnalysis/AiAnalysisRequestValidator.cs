namespace Freeqy_APIs.Contracts.AiAnalysis;

public class AiAnalysisRequestValidator : AbstractValidator<AiAnalysisRequest>
{
	public AiAnalysisRequestValidator()
	{
		RuleFor(x => x.ProjectIdea)
			.NotEmpty()
			.WithMessage("Project idea is required")
			.MinimumLength(10)
			.WithMessage("Project idea must be at least 10 characters")
			.MaximumLength(2000)
			.WithMessage("Project idea cannot exceed 2000 characters");
	}
}
