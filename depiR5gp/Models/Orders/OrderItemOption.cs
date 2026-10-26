namespace depiR5gp.Models.Orders;

public class OrderItemOption
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int OrderItemLineNo { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; } = 0.00m;

    // Navigation Property
    public virtual OrderItem OrderItem { get; set; } = null!;
}

