using depiR5gp.Models.Orders;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.ReviewsLoyalty;

public class PointsLedger
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public int Points { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Order? Order { get; set; }
}

