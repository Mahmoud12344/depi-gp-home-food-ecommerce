using Microsoft.AspNetCore.Identity;
using depiR5gp.Models.AIAssistant;
using depiR5gp.Models.Cart;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Messaging;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Refunds;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Support;

namespace depiR5gp.Models.Users;

public class AppUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsSuspended { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public int PointsBalance { get; set; } = 0;

    // Navigation Properties
    public virtual ChefProfile? ChefProfile { get; set; }
    public virtual ICollection<Cart.Cart> Carts { get; set; } = new List<Cart.Cart>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<PointsLedger> PointsLedgerEntries { get; set; } = new List<PointsLedger>();
    public virtual ICollection<Conversation> CustomerConversations { get; set; } = new List<Conversation>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<SupportCase> SupportCases { get; set; } = new List<SupportCase>();
    public virtual ICollection<CaseMessage> CaseMessages { get; set; } = new List<CaseMessage>();
    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<AssistantConversation> AssistantConversations { get; set; } = new List<AssistantConversation>();
    public virtual ICollection<AssistantUsage> AssistantUsages { get; set; } = new List<AssistantUsage>();
}

