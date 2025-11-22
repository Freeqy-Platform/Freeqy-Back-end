namespace Freeqy_APIs.Contracts.Projects;

public record SimpleUserDto(
    long UserId,
    string Name
);

public record CategoryDto(
    long CategoryId,
    string Name
);

public record TagDto(
    long TagId,
    string Name
);

public record TechnologyDto(
    long TechnologyId,
    string Name
);

public record ProjectListItemResponse(
    long ProjectId,
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
