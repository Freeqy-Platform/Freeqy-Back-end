using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public record ProjectTargetRequest(
    [property: JsonPropertyName("project_idea")] string ProjectIdea,
    [property: JsonPropertyName("job_role")] string JobRole
    
);
public record InternalAnalysis
{
    [JsonPropertyName("technical_pillars")]
    public List<TechnicalPillar> TechnicalPillars { get; set; } = [];
}

public class TechnicalPillar
{
    [JsonPropertyName("pillar_name")]
    public string PillarName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class InterviewDetails
{
    [JsonPropertyName("opening_statement")]
    public string OpeningStatement { get; set; } = string.Empty;

    [JsonPropertyName("questions")]
    public List<InterviewQuestion> Questions { get; set; } = [];
}

public class InterviewQuestion
{
    [JsonPropertyName("question_number")]
    public int QuestionNumber { get; set; }

    [JsonPropertyName("technical_question")]
    public string? TechnicalQuestion { get; set; }

    [JsonPropertyName("scenario_based_question")]
    public string? ScenarioBasedQuestion { get; set; }
}

public class AnalyzeProjectResponse
{
    [JsonPropertyName("internal_analysis")]
    public InternalAnalysis InternalAnalysis { get; set; } = new();

    [JsonPropertyName("interview")]
    public InterviewDetails Interview { get; set; } = new();
}
