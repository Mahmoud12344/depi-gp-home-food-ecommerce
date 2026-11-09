namespace depiR5gp.ViewModels;

public sealed class CartPageVM
{
    public List<CartPageItemVM> Items { get; set; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public int TotalQuantity => Items.Sum(item => item.Quantity);
}

public sealed class CartPageItemVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string? PresetKey { get; set; }
    public bool IsAvailable { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
