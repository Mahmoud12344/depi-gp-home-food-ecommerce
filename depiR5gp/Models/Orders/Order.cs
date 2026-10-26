using depiR5gp.Models.Chefs;
using depiR5gp.Models.Payments;
using depiR5gp.Models.Refunds;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Support;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Orders;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChefProfileId { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentStatus { get; set; } = "Pending";
    public string Reference { get; set; } = string.Empty;
    public string? SubmissionToken { get; set; }
    public decimal Subtotal { get; set; }
    public decimal PointsDiscount { get; set; } = 0.00m;
    public decimal Total { get; set; }
    public decimal CommissionRate { get; set; } = 0.1500m;
    public decimal CommissionAmount { get; set; } = 0.00m;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual Review? Review { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    public virtual ICollection<PointsLedger> PointsLedgerEntries { get; set; } = new List<PointsLedger>();
    public virtual ICollection<SupportCase> SupportCases { get; set; } = new List<SupportCase>();
}

