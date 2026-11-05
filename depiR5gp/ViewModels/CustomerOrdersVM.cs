namespace depiR5gp.ViewModels;

public class CustomerOrdersVM
{
    public CustomerProfileVM Profile { get; set; } = new();
    public IReadOnlyList<CustomerOrderVM> Orders { get; set; } = [];
}

public class CustomerOrderVM
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ChefName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<CustomerOrderItemVM> Items { get; set; } = [];
}

public class CustomerOrderItemVM
{
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
