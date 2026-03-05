namespace Freeqy_APIs.Entities;

public class Conversation
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();
    public ConversationType Type { get; set; }
    public string? Title { get; set; }
    public string? ChannelName { get; set; }

    public string? ProjectId { get; set; }
    public Project? Project { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
