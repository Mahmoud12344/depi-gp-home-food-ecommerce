using depiR5gp.Models.Menu;

namespace depiR5gp.Models.Cart;

public class CartItemOption
{
    public int Id { get; set; }
    public int CartItemId { get; set; }
    public int DishOptionId { get; set; }

    // Navigation Properties
    public virtual CartItem CartItem { get; set; } = null!;
    public virtual DishOption DishOption { get; set; } = null!;
}

