using depiR5gp.Models.Users;

namespace depiR5gp.Models.Support;

public class Faq
{
    public int Id { get; set; }
    public int? UpdatedByUserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual AppUser? UpdatedByUser { get; set; }
}

