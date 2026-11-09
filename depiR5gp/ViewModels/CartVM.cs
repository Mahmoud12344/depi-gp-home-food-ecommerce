namespace depiR5gp.ViewModels;

public sealed class CartVM
{
    public IReadOnlyList<CartLineVM> Items { get; init; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public bool IsEmpty => Items.Count == 0;
}

public sealed class CartLineVM
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
    public string? PresetKey { get; init; }
    public bool IsAvailable { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
}
