using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TeamStructureResponse(
	List<RoleRecommendation> Roles
);

public sealed record RoleRecommendation(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("track")] string Track,
	[property: JsonPropertyName("recommended_members")] int RecommendedMembers,
	[property: JsonPropertyName("skills")] List<string> Skills,
	[property: JsonPropertyName("priority")] string Priority
);
