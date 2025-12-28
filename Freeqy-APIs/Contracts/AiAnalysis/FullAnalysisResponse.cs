namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record FullAnalysisResponse(
	TeamStructureResponse TeamStructure,
	TechStackResponse TechStack,
	string ProjectIdea,
	DateTime AnalyzedAt,
	AnalysisMetrics Metrics
);

public sealed record AnalysisMetrics(
	int TeamSize,
	int TechnologiesCount,
	string ProjectComplexity,
	string EstimatedDuration
);
