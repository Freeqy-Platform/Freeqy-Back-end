namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TechStackResponse(
	List<string> Backend,
	List<string> Frontend,
	List<string> Database,
	List<string> AiStack,
	List<string> DevOps
);
