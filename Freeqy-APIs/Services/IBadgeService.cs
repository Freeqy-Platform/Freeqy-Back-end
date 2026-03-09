using Freeqy_APIs.Contracts.Badges;

namespace Freeqy_APIs.Services;

public interface IBadgeService
{
    Task<Result<IEnumerable<BadgeResponse>>> GetUserBadgesAsync(string userId, CancellationToken cancellationToken = default);
    Task AssignBadgesAsync(string userId, CancellationToken cancellationToken = default);
    Task AssignBadgesForAllUsersAsync(CancellationToken cancellationToken = default);
}
