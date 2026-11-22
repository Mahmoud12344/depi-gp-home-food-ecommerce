using System.Text.Json;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Models.Menu;

public class DishCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string CuisineName { get; set; } = "";
    public int ChefId { get; set; }
    public string KitchenName { get; set; } = "";
    public string? Area { get; set; }
    public bool IsAvailable { get; set; }
    public double Rating { get; set; }       // متوسط تقييمات الشيف (الـ Review مربوط بالشيف مش بالطبق)
    public int ReviewCount { get; set; }

    public string AreaName => PickupAreas.NameOf(Area);
    public string QuickViewJson => JsonSerializer.Serialize(new
    {
        id = Id,
        name = Name,
        price = Price,
        rating = Rating,
        reviewCount = ReviewCount,
        kitchenName = KitchenName,
        category = CuisineName,
        pickupArea = AreaName,
        isAvailable = IsAvailable,
        image = ImageUrl
    });
    public string OptionsJson => JsonSerializer.Serialize(new { id = Id, name = Name, price = Price, kitchenName = KitchenName });
}