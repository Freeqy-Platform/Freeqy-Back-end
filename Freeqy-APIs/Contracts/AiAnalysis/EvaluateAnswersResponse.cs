using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public record EvaluateAnswersResponse
{
    [JsonPropertyName("evaluation")]
    public EvaluationDetails Evaluation { get; set; } = new();
}

public class EvaluationDetails
{
    [JsonPropertyName("technical_skill_score")]
    public string TechnicalSkillScore { get; set; } = string.Empty;

    [JsonPropertyName("project_understanding_score")]
    public string ProjectUnderstandingScore { get; set; } = string.Empty;

    [JsonPropertyName("final_acceptance_percentage")]
    public string FinalAcceptancePercentage { get; set; } = string.Empty;

    [JsonPropertyName("feedback")]
    public FeedbackDetails Feedback { get; set; } = new();
}

public class FeedbackDetails
{
    [JsonPropertyName("strengths")]
    public string Strengths { get; set; } = string.Empty;

    [JsonPropertyName("weaknesses")]
    public string Weaknesses { get; set; } = string.Empty;

    [JsonPropertyName("improvements")]
    public string Improvements { get; set; } = string.Empty;
}
