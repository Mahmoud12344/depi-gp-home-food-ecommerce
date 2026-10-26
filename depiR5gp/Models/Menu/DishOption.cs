using depiR5gp.Models.Cart;

namespace depiR5gp.Models.Menu;

public class DishOption
{
    public int Id { get; set; }
    public int DishId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; } = 0.00m;
    public bool IsRequired { get; set; } = false;

    // Navigation Properties
    public virtual Dish Dish { get; set; } = null!;
    public virtual ICollection<CartItemOption> CartItemOptions { get; set; } = new List<CartItemOption>();
}

