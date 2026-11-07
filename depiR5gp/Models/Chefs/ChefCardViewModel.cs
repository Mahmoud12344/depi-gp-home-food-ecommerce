namespace depiR5gp.Models.Chefs;

public class ChefCardViewModel
{
    public int Id { get; set; }
    public string KitchenName { get; set; } = "";
    public string ChefName { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public string? Area { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string AreaName => PickupAreas.NameOf(Area);
}