namespace Freeqy_APIs.Contracts.Projects;

public record ProjectRequest
(
    string Name,
    string Description,
    ProjectStatus Status,
    ProjectVisibility Visibility,
    List<string> TechnologyIds,
    TimeSpan EstimatedTime,
    string CategoryId
);