namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record FullAnalysisResponse(
	bool Success,
	TeamStructureResponse TeamStructure,
	TechStackResponse TechStack,
	int TotalRoles,
	int TotalMembers,
	double ProcessingTime
);
