namespace depiR5gp.Models.Users;

public class UserStatusHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ChangedByUserId { get; set; }
    public UserStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual AppUser? ChangedByUser { get; set; }
}
