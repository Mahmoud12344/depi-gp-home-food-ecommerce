namespace depiR5gp.Models.Chefs;

public class ChefProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public List<string> WorkingHours { get; set; } = new List<string>();
    public bool IsAcceptingOrders { get; set; }
}