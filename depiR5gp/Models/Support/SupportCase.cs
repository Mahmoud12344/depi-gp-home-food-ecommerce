using depiR5gp.Models.Orders;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Support;

public class SupportCase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string RequesterRole { get; set; } = string.Empty;
    public bool AwaitingSupport { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual ICollection<CaseMessage> Messages { get; set; } = new List<CaseMessage>();
}

