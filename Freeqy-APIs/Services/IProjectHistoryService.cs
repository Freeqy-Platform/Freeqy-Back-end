using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Services;

public interface IProjectHistoryService
{
    Task RecordEventAsync(
        string userId,
        string projectId,
        string projectName,
        string projectCategory,
        HistoryEventType eventType,
        string? role = null,
        ProjectStatus? projectStatusAtEvent = null,
        CancellationToken ct = default);

    Task<Result<ProjectTimelineResponse>> GetUserTimelineAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<Result<UserProjectStats>> GetUserStatsAsync(
        string userId,
        CancellationToken ct = default);
}
