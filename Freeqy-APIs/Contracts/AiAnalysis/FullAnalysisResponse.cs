using System.Text.Json.Serialization;

public sealed record FullAnalysisResponse(
    [property: JsonPropertyName("success")] bool Success,

    [property: JsonPropertyName("team_structure")]
    TeamStructureResponse TeamStructure,

    [property: JsonPropertyName("tech_stack")]
    TechStackResponse TechStack,

    [property: JsonPropertyName("total_roles")] int TotalRoles,
    [property: JsonPropertyName("total_members")] int TotalMembers,
    [property: JsonPropertyName("processing_time")] double ProcessingTime
);