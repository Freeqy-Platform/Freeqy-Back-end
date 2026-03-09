namespace Freeqy_APIs.Contracts.Badges;

public record BadgeResponse(
    int Id,
    string Name,
    string Description,
    string Type,
    DateTime EarnedAt
);
