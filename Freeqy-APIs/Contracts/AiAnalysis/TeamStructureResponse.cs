namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TeamStructureResponse(
	List<RoleRecommendation> Roles,
	List<string> RequiredSkills
);

public sealed record RoleRecommendation(
	string Role,
	string Track,
	int RecommendedMembers,
	List<string> Skills,
	string Priority
);
