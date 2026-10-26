using depiR5gp.Models.Menu;

namespace depiR5gp.Models.Cart;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int DishId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }

    // Navigation Properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Dish Dish { get; set; } = null!;
    public virtual ICollection<CartItemOption> Options { get; set; } = new List<CartItemOption>();
}

