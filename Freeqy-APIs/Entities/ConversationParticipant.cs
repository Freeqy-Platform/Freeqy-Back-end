namespace Freeqy_APIs.Entities;

public class ConversationParticipant
{
    public string ConversationId { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public ParticipantRole Role { get; set; } = ParticipantRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }

    /// <summary>If true, the user will not receive notifications for new messages in this conversation.</summary>
    public bool IsMuted { get; set; } = false;
}
