namespace Freeqy_APIs.Entities;

public class MessageReadReceipt
{
    public string MessageId { get; set; } = null!;
    public Message Message { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
