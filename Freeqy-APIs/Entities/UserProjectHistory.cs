namespace Freeqy_APIs.Entities;

public class UserProjectHistory
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCategory { get; set; } = string.Empty;
    public HistoryEventType EventType { get; set; }
    public string? Role { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public ProjectStatus? ProjectStatusAtEvent { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public Project Project { get; set; } = default!;
}
