using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Models.AIAssistant;
using depiR5gp.Models.Cart;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Messaging;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Payments;
using depiR5gp.Models.Platform;
using depiR5gp.Models.Refunds;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Support;
using depiR5gp.Models.Users;

namespace depiR5gp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<int>, int>(options)
{
    // Chefs Module
    public DbSet<ChefProfile> ChefProfiles => Set<ChefProfile>();
    public DbSet<ChefDocument> ChefDocuments => Set<ChefDocument>();
    public DbSet<ChefWorkingHours> ChefWorkingHours => Set<ChefWorkingHours>();

    // Menu Module
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<DishOption> DishOptions => Set<DishOption>();

    // Cart Module
    public DbSet<Models.Cart.Cart> Carts => Set<Models.Cart.Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<CartItemOption> CartItemOptions => Set<CartItemOption>();

    // Orders Module
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemOption> OrderItemOptions => Set<OrderItemOption>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    // Payments Module
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    // Refunds Module
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    // Reviews & Loyalty Module
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<PointsLedger> PointsLedgers => Set<PointsLedger>();

    // Messaging Module
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Support Module
    public DbSet<SupportCase> SupportCases => Set<SupportCase>();
    public DbSet<CaseMessage> CaseMessages => Set<CaseMessage>();
    public DbSet<Faq> Faqs => Set<Faq>();

    // AI Assistant Module
    public DbSet<AssistantConversation> AssistantConversations => Set<AssistantConversation>();
    public DbSet<AssistantMessage> AssistantMessages => Set<AssistantMessage>();
    public DbSet<AssistantUsage> AssistantUsages => Set<AssistantUsage>();

    // Platform Module
    public DbSet<PlatformSettings> PlatformSettings => Set<PlatformSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity type configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
