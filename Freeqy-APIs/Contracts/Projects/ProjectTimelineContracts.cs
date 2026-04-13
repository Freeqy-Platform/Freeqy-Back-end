namespace Freeqy_APIs.Contracts.Projects;

public record ProjectTimelineEntry(
    long Id,
    string ProjectId,
    string ProjectName,
    string ProjectCategory,
    string EventType,
    string? Role,
    DateTime EventDate,
    string? ProjectStatus
);

public record UserProjectStats(
    int TotalProjectsJoined,
    int ProjectsAsOwner,
    int ProjectsAsMember,
    int CompletedProjects,
    double CompletionRate,
    DateTime? FirstProjectDate,
    DateTime? LastActiveDate
);

public record ProjectTimelineResponse(
    List<ProjectTimelineEntry> Events,
    int TotalCount,
    UserProjectStats Stats
);
