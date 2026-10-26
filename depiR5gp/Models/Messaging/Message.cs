using depiR5gp.Models.Users;

namespace depiR5gp.Models.Messaging;

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Navigation Properties
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual AppUser SenderUser { get; set; } = null!;
}

