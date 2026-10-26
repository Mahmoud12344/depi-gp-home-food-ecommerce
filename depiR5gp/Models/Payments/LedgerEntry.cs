using depiR5gp.Models.Chefs;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Refunds;

namespace depiR5gp.Models.Payments;

public class LedgerEntry
{
    public int Id { get; set; }
    public int ChefProfileId { get; set; }
    public int? OrderId { get; set; }
    public int? PayoutId { get; set; }
    public int? RefundRequestId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual Payout? Payout { get; set; }
    public virtual RefundRequest? RefundRequest { get; set; }
}

