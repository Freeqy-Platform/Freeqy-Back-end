namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TeamStructureResponse(
	List<RoleRecommendation> Roles,
	int TotalMembers,
	List<string> RequiredSkills,
	string ProjectComplexity,
	string EstimatedDuration
);

public sealed record RoleRecommendation(
	string RoleName,
	string Track,
	int Count,
	List<string> RequiredSkills,
	string Priority,
	string Description
);
