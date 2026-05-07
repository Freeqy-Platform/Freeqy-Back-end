using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class Meeting
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProjectId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? MeetingLink { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}
