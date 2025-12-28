namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TechStackResponse(
	List<TechnologyRecommendation> Backend,
	List<TechnologyRecommendation> Frontend,
	List<TechnologyRecommendation> Database,
	List<TechnologyRecommendation> DevOps,
	List<TechnologyRecommendation> AiStack,
	string ArchitecturePattern
);

public sealed record TechnologyRecommendation(
	string Name,
	string Category,
	string Reason,
	string Priority
);
