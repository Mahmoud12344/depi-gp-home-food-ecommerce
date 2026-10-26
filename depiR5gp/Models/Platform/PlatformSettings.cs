using depiR5gp.Models.Users;

namespace depiR5gp.Models.Platform;

public class PlatformSettings
{
    public int Id { get; set; }
    public int? UpdatedByUserId { get; set; }
    public decimal CommissionRate { get; set; } = 0.1500m;
    public int RefundWindowHours { get; set; } = 24;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual AppUser? UpdatedByUser { get; set; }
}

