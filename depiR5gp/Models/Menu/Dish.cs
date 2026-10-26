using depiR5gp.Models.Cart;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Orders;

namespace depiR5gp.Models.Menu;

public class Dish
{
    public int Id { get; set; }
    public int ChefProfileId { get; set; }
    public int CategoryId { get; set; }
    public int CuisineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsArchived { get; set; } = false;

    // Navigation Properties
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual Cuisine Cuisine { get; set; } = null!;
    public virtual ICollection<DishOption> DishOptions { get; set; } = new List<DishOption>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

