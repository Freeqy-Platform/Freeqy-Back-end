using System.ComponentModel.DataAnnotations.Schema;
using Freeqy_APIs.Enums;

namespace Freeqy_APIs.Entities;

public class Meeting
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProjectId { get; set; }
    public string CreatedByUserId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public MeetingType Type { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }
}
