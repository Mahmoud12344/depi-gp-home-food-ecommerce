using depiR5gp.Models.Orders;

namespace depiR5gp.Models.Payments;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? StripeReference { get; set; }
    public decimal UnsettledAmount { get; set; } = 0.00m;

    // Navigation Property
    public virtual Order Order { get; set; } = null!;
}

