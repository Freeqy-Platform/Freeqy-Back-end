namespace Freeqy_APIs.Contracts.Projects;

public record SimpleUserDto(
    string Id,
    string Name
);

public record CategoryDto(
    string Id,
    string Name
);

public record TechnologyDto(
    string Id,
    string Name
);

public record ProjectListItemResponse(
    string Id,
    string Name,
    string Description,
    SimpleUserDto Owner,
    CategoryDto Category,
    string Status,
    string Visibility,
    string EstimatedTime,
    IReadOnlyList<TechnologyDto> Technologies,
    int MembersCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ProjectListResponse(
    IReadOnlyList<ProjectListItemResponse> Projects
);
