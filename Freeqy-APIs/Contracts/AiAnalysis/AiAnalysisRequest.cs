namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record AiAnalysisRequest(
	[Required(ErrorMessage = "Project idea is required")]
	[StringLength(2000, MinimumLength = 10, ErrorMessage = "Project idea must be between 10 and 2000 characters")]
	string ProjectIdea
);
