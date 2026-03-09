using Freeqy_APIs.Contracts.Badges;
using Freeqy_APIs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Freeqy_APIs.Services;

public class BadgeService(ApplicationDbContext context) : IBadgeService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<BadgeResponse>>> GetUserBadgesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
            return Result.Failure<IEnumerable<BadgeResponse>>(BadgeErrors.UserNotFound);

        var badges = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .AsNoTracking()
            .Select(ub => new BadgeResponse(
                ub.Badge.Id,
                ub.Badge.Name,
                ub.Badge.Description,
                ub.Badge.Type.ToString(),
                ub.EarnedAt
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BadgeResponse>>(badges);
    }

    public async Task AssignBadgesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserBadges)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return;

        var earnedBadgeIds = user.UserBadges.Select(ub => ub.BadgeId).ToHashSet();
        var toAssign = new List<UserBadge>();

        // FirstProject: Created at least 1 project
        if (!earnedBadgeIds.Contains((int)BadgeType.FirstProject))
        {
            var hasProject = await _context.Projects
                .AnyAsync(p => p.OwnerId == userId && p.DeletedAt == null, cancellationToken);
            if (hasProject)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.FirstProject });
        }

        // TeamPlayer: Member of at least 5 projects
        if (!earnedBadgeIds.Contains((int)BadgeType.TeamPlayer))
        {
            var joinedCount = await _context.ProjectMembers
                .CountAsync(pm => pm.UserId == userId, cancellationToken);
            if (joinedCount >= 5)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.TeamPlayer });
        }

        // ProjectCompleter: Part of at least 3 completed projects
        if (!earnedBadgeIds.Contains((int)BadgeType.ProjectCompleter))
        {
            var completedCount = await _context.ProjectMembers
                .CountAsync(pm => pm.UserId == userId && pm.Project.Status == ProjectStatus.Completed, cancellationToken);
            if (completedCount >= 3)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.ProjectCompleter });
        }

        // EarlyAdopter: Among the first 100 registered users (UUID v7 is time-ordered)
        if (!earnedBadgeIds.Contains((int)BadgeType.EarlyAdopter))
        {
            var isEarlyAdopter = await _context.Users
                .OrderBy(u => u.Id)
                .Take(100)
                .AnyAsync(u => u.Id == userId, cancellationToken);
            if (isEarlyAdopter)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.EarlyAdopter });
        }

        // Mentor: Owned at least 5 projects
        if (!earnedBadgeIds.Contains((int)BadgeType.Mentor))
        {
            var ownedCount = await _context.Projects
                .CountAsync(p => p.OwnerId == userId && p.DeletedAt == null, cancellationToken);
            if (ownedCount >= 5)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.Mentor });
        }

        // TopContributor: Computed score > 500
        if (!earnedBadgeIds.Contains((int)BadgeType.TopContributor))
        {
            var score = await ComputeScoreAsync(userId, cancellationToken);
            if (score > 500)
                toAssign.Add(new UserBadge { UserId = userId, BadgeId = (int)BadgeType.TopContributor });
        }

        if (toAssign.Count > 0)
        {
            _context.UserBadges.AddRange(toAssign);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AssignBadgesForAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var userIds = await _context.Users
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await AssignBadgesAsync(userId, cancellationToken);
        }
    }

    // Score: owned(×100) + joined(×50) + completedOwned(×150) + completedMember(×75)
    private async Task<int> ComputeScoreAsync(string userId, CancellationToken cancellationToken)
    {
        var ownedCount = await _context.Projects
            .CountAsync(p => p.OwnerId == userId && p.DeletedAt == null, cancellationToken);

        var memberCount = await _context.ProjectMembers
            .CountAsync(pm => pm.UserId == userId, cancellationToken);

        var completedOwnedCount = await _context.Projects
            .CountAsync(p => p.OwnerId == userId && p.Status == ProjectStatus.Completed && p.DeletedAt == null, cancellationToken);

        var completedMemberCount = await _context.ProjectMembers
            .CountAsync(pm => pm.UserId == userId && pm.Project.Status == ProjectStatus.Completed, cancellationToken);

        return (ownedCount * 100) + (memberCount * 50) + (completedOwnedCount * 150) + (completedMemberCount * 75);
    }
}
