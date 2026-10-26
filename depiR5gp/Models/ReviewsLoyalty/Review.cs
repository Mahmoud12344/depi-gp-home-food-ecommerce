using depiR5gp.Models.Chefs;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.ReviewsLoyalty;

public class Review
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public int ChefProfileId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public string ModerationStatus { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual ChefProfile ChefProfile { get; set; } = null!;
}

