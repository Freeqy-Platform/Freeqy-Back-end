using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Freeqy_APIs.Contracts.AiAnalysis;

public record EvaluateAnswersRequest(
    [property: Required]
    [property: JsonPropertyName("questions_and_answers")]
    List<QuestionAnswerDto> QuestionsAndAnswers
);

public class QuestionAnswerDto
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;
}
