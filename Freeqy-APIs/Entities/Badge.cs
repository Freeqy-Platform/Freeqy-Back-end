namespace Freeqy_APIs.Entities;

public class Badge
{
    public int Id { get; set; }
    public BadgeType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserBadge> UserBadges { get; set; } = [];
}
