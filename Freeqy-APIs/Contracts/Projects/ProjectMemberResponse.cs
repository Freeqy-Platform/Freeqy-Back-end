namespace Freeqy_APIs.Contracts.Projects;

public record ProjectMemberDto(
    string UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhotoUrl,
    string Role,
    bool IsActive,
    DateTime JoinedAt
);

public record ProjectMembersResponse(
    IReadOnlyList<ProjectMemberDto> Members
);
