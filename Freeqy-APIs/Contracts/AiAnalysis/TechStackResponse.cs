using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public sealed record TechStackResponse(
	[property: JsonPropertyName("backend")] List<string> Backend,
	[property: JsonPropertyName("frontend")] List<string> Frontend,
	[property: JsonPropertyName("database")] List<string> Database,
	[property: JsonPropertyName("ai_stack")] List<string> AiStack,
	[property: JsonPropertyName("devops")] List<string> DevOps
);
