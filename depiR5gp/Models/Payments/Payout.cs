using depiR5gp.Models.Chefs;

namespace depiR5gp.Models.Payments;

public class Payout
{
    public int Id { get; set; }
    public int ChefProfileId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reference { get; set; }
    public string? IdempotencyKey { get; set; }
    public bool IsSimulated { get; set; } = false;

    // Navigation Properties
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}

