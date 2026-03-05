namespace Freeqy_APIs.Entities;

public class Message
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();

    public string ConversationId { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;

    public string SenderId { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
}
