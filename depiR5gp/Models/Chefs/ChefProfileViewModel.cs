namespace depiR5gp.Models.Chefs;

public class ChefPublicReviewItemVM
{
    public string CustomerName { get; set; } = "Verified Customer";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChefProfileViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public List<string> WorkingHours { get; set; } = new List<string>();
    public bool IsAcceptingOrders { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ChefPublicReviewItemVM> Reviews { get; set; } = new();
}