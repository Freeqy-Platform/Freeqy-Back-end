namespace Freeqy_APIs.Contracts.Projects;

public record SimpleUserDto(
    string UserId,
    string Name
);

public record CategoryDto(
    string CategoryId,
    string Name
);

public record TagDto(
    string TagId,
    string Name
);

public record TechnologyDto(
    string TechnologyId,
    string Name
);

public record ProjectListItemResponse(
    string ProjectId,
    string Name,
    string Description,
    SimpleUserDto Owner,
    CategoryDto Category,
    string Status,
    string Visibility,
    string EstimatedTime,
    IReadOnlyList<TagDto> Tags,
    IReadOnlyList<TechnologyDto> Technologies,
    int MembersCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ProjectListResponse(
    IReadOnlyList<ProjectListItemResponse> Projects
);
