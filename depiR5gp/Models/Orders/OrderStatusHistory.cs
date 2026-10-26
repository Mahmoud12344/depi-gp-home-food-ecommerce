using depiR5gp.Models.Users;

namespace depiR5gp.Models.Orders;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? ChangedByUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual AppUser? ChangedByUser { get; set; }
}

