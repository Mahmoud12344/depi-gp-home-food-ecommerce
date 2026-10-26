using depiR5gp.Models.Users;

namespace depiR5gp.Models.AIAssistant;

public class AssistantUsage
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly UsageDate { get; set; }
    public int RequestCount { get; set; } = 0;

    // Navigation Property
    public virtual AppUser User { get; set; } = null!;
}

