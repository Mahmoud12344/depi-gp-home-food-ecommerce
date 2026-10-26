using depiR5gp.Models.Orders;
using depiR5gp.Models.Payments;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Refunds;

public class RefundRequest
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Requested";
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string? Decision { get; set; }
    public string? DecisionReason { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? StripeRefundRef { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}

