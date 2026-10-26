namespace depiR5gp.Models.AIAssistant;

public class AssistantMessage
{
    public int Id { get; set; }
    public int AssistantConversationId { get; set; }
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual AssistantConversation AssistantConversation { get; set; } = null!;
}

