using depiR5gp.Models.Cart;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Messaging;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Payments;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Chefs;

public class ChefProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public bool HasAcceptedTerms { get; set; } = false;
    public decimal? CommissionRate { get; set; }

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<ChefDocument> Documents { get; set; } = new List<ChefDocument>();
    public virtual ICollection<ChefWorkingHours> WorkingHours { get; set; } = new List<ChefWorkingHours>();
    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    public virtual ICollection<Cart.Cart> Carts { get; set; } = new List<Cart.Cart>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}

