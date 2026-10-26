using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Messaging;

public class Conversation
{
    public int Id { get; set; }
    public int CustomerUserId { get; set; }
    public int ChefProfileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual AppUser CustomerUser { get; set; } = null!;
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

