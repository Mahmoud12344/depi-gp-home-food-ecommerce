namespace depiR5gp.Models.Chefs;

public class ChefDocument
{
    public int Id { get; set; }
    public int ChefProfileId { get; set; }
    public string DocType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual ChefProfile ChefProfile { get; set; } = null!;
}

