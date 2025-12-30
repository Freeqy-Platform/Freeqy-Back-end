using Freeqy_APIs.Contracts.Common;

namespace Freeqy_APIs.Contracts.Projects;

public record ProjectRequestFilter(
	string? OwnerId,
	string? Category,
	ProjectStatus? Status,
	ProjectVisibility? Visibility,
	string? Tech,
	int PageNumber = 1,
	int PageSize = 10
) : RequestFilters
{
	public int PageNumber { get; init; } = PageNumber;
	public int PageSize { get; init; } = PageSize;
}
