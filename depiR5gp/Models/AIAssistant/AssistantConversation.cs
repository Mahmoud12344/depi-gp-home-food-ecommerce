using depiR5gp.Models.Users;

namespace depiR5gp.Models.AIAssistant;

public class AssistantConversation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<AssistantMessage> Messages { get; set; } = new List<AssistantMessage>();
}

