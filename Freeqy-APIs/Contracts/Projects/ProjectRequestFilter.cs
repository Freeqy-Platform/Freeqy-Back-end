namespace Freeqy_APIs.Contracts.Projects;

public record ProjectRequestFilter(
	string? OwnerId,
	string? Category,
	ProjectStatus? Status,
	ProjectVisibility? Visibility,
	string? Tech
);
