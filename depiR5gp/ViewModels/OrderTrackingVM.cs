using depiR5gp.Models.Orders;

namespace depiR5gp.ViewModels;

public sealed class OrderTrackingVM
{
    public Order Order { get; init; } = null!;
    public string KitchenName => Order.ChefProfile?.KitchenName ?? "Unknown chef";
    public string KitchenAddress => Order.ChefProfile?.Address ?? Order.DeliveryAddress;
}
