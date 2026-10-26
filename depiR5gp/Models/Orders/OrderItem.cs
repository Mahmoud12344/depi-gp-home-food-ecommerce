using depiR5gp.Models.Menu;

namespace depiR5gp.Models.Orders;

public class OrderItem
{
    public int OrderId { get; set; }
    public int LineNo { get; set; }
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual Dish Dish { get; set; } = null!;
    public virtual ICollection<OrderItemOption> Options { get; set; } = new List<OrderItemOption>();
}

