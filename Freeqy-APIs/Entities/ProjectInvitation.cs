using System.ComponentModel.DataAnnotations.Schema;

namespace Freeqy_APIs.Entities;

public class ProjectInvitation
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();
    
    public string ProjectId { get; set; }
    
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; }
    
    public string InvitedEmail { get; set; }
    
    public string? InvitedUserId { get; set; }
    
    [ForeignKey(nameof(InvitedUserId))]
    public ApplicationUser? InvitedUser { get; set; }
    
    public string SentByUserId { get; set; }
    
    [ForeignKey(nameof(SentByUserId))]
    public ApplicationUser SentByUser { get; set; }
    
    public ProjectInvitationStatus Status { get; set; } = ProjectInvitationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    
    public DateTime? RespondedAt { get; set; }
    
    public string? RespondedReason { get; set; }
}
