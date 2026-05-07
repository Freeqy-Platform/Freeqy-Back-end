using Freeqy_APIs.Enums;

namespace Freeqy_APIs.Contracts.Meetings;

public record CreateMeetingRequest
{
    public string ProjectId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public MeetingType Type { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
}

public record UpdateMeetingRequest
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public MeetingType Type { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
}

public record MeetingResponse
{
    public string Id { get; set; }
    public string ProjectId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public MeetingType Type { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
