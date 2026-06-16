using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public record AnalyzeProjectRequest(
    [property: Required]
    [property: JsonPropertyName("project_idea")]
    string ProjectIdea,

    [property: Required]
    [property: JsonPropertyName("job_role")]
    string JobRole
);