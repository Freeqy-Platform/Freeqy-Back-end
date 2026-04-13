using Freeqy_APIs.Contracts.Projects;

namespace Freeqy_APIs.Services;

public class ProjectHistoryService(ApplicationDbContext context) : IProjectHistoryService
{
    private readonly ApplicationDbContext _context = context;

    public async Task RecordEventAsync(
        string userId,
        string projectId,
        string projectName,
        string projectCategory,
        HistoryEventType eventType,
        string? role = null,
        ProjectStatus? projectStatusAtEvent = null,
        CancellationToken ct = default)
    {
        if (eventType == HistoryEventType.ProjectCompleted)
        {
            var hasCompletedEvent = await _context.UserProjectHistories
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId
                               && x.ProjectId == projectId
                               && x.EventType == HistoryEventType.ProjectCompleted, ct);

            if (hasCompletedEvent)
                return;
        }

        var historyEntry = new UserProjectHistory
        {
            UserId = userId,
            ProjectId = projectId,
            ProjectName = projectName,
            ProjectCategory = projectCategory,
            EventType = eventType,
            Role = role,
            EventDate = DateTime.UtcNow,
            ProjectStatusAtEvent = projectStatusAtEvent
        };

        await _context.UserProjectHistories.AddAsync(historyEntry, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result<ProjectTimelineResponse>> GetUserTimelineAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, ct);

        if (!userExists)
            return Result.Failure<ProjectTimelineResponse>(ProjectHistoryErrors.UserNotFound);

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _context.UserProjectHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EventDate);

        var totalCount = await query.CountAsync(ct);

        var events = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProjectTimelineEntry(
                x.Id,
                x.ProjectId,
                x.ProjectName,
                x.ProjectCategory,
                x.EventType.ToString(),
                x.Role,
                x.EventDate,
                x.ProjectStatusAtEvent.HasValue ? x.ProjectStatusAtEvent.Value.ToString() : null
            ))
            .ToListAsync(ct);

        var statsResult = await GetUserStatsAsync(userId, ct);
        if (statsResult.IsFailure)
            return Result.Failure<ProjectTimelineResponse>(statsResult.Error);

        return Result.Success(new ProjectTimelineResponse(events, totalCount, statsResult.Value));
    }

    public async Task<Result<UserProjectStats>> GetUserStatsAsync(string userId, CancellationToken ct = default)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, ct);

        if (!userExists)
            return Result.Failure<UserProjectStats>(ProjectHistoryErrors.UserNotFound);

        var historyEntries = await _context.UserProjectHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                x.ProjectId,
                x.EventType,
                x.Role,
                x.EventDate
            })
            .ToListAsync(ct);

        var joinedEntries = historyEntries
            .Where(x => x.EventType == HistoryEventType.Joined)
            .ToList();

        var totalProjectsJoined = joinedEntries
            .Select(x => x.ProjectId)
            .Distinct()
            .Count();

        var projectsAsOwner = joinedEntries
            .Where(x => string.Equals(x.Role, "Owner", StringComparison.OrdinalIgnoreCase)
                        || (x.Role?.Contains("owner", StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(x => x.ProjectId)
            .Distinct()
            .Count();

        var completedProjects = historyEntries
            .Where(x => x.EventType == HistoryEventType.ProjectCompleted)
            .Select(x => x.ProjectId)
            .Distinct()
            .Count();

        var firstProjectDate = joinedEntries
            .Select(x => (DateTime?)x.EventDate)
            .Min();

        var lastActiveDate = historyEntries
            .Select(x => (DateTime?)x.EventDate)
            .Max();

        var stats = new UserProjectStats(
            TotalProjectsJoined: totalProjectsJoined,
            ProjectsAsOwner: projectsAsOwner,
            ProjectsAsMember: totalProjectsJoined - projectsAsOwner,
            CompletedProjects: completedProjects,
            CompletionRate: totalProjectsJoined > 0
                ? Math.Round((double)completedProjects / totalProjectsJoined * 100, 1)
                : 0,
            FirstProjectDate: firstProjectDate,
            LastActiveDate: lastActiveDate
        );

        return Result.Success(stats);
    }
}
