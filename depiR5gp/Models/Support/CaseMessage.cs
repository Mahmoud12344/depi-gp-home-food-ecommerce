using depiR5gp.Models.Users;

namespace depiR5gp.Models.Support;

public class CaseMessage
{
    public int Id { get; set; }
    public int SupportCaseId { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorRole { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual SupportCase SupportCase { get; set; } = null!;
    public virtual AppUser AuthorUser { get; set; } = null!;
}

