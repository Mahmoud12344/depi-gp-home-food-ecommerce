using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds realistic demo orders distributed across the last 7 days, previous week, previous month,
/// and earlier months of the year so that Admin KPI cards, growth percentages, financial breakdown,
/// and the SVG sales velocity trend chart have rich data across all date range filters.
/// </summary>
public class DemoOrdersKpiSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Demo;
    public int Order => 25; // Runs after DemoUsersSeeder (Order = 10)

    private sealed record DemoOrderSeed(
        string Reference,
        string CustomerEmailPrefix,
        string ChefEmailPrefix,
        decimal Total,
        string PaymentMethod,
        string Status,
        int DaysAgo);

    public async Task SeedAsync(SeedContext context)
    {
        var domain = context.Options.DemoEmailDomain.TrimStart('@');

        var seeds = new List<DemoOrderSeed>
        {
            // Last 7 days (daily velocity curve + active in-progress orders)
            new("ORD-KPI-001", "customer.sara",   "chef.hassan", 420.00m, "Card", "Pending",        0),
            new("ORD-KPI-002", "customer.mariam", "chef.layla",  310.00m, "Card", "Preparing",      0),
            new("ORD-KPI-003", "customer.ahmed",  "chef.tarek",  540.00m, "Cash", "Delivered",      0),
            new("ORD-KPI-004", "customer.fatma",  "chef.mona",   285.00m, "Card", "Delivered",      1),
            new("ORD-KPI-005", "customer.omar",   "chef.hassan", 390.00m, "Cash", "Delivered",      1),
            new("ORD-KPI-006", "customer.sara",   "chef.layla",  460.00m, "Card", "Delivered",      2),
            new("ORD-KPI-007", "customer.mariam", "chef.tarek",  610.00m, "Card", "Delivered",      3),
            new("ORD-KPI-008", "customer.ahmed",  "chef.hassan", 350.00m, "Cash", "OutForDelivery", 3),
            new("ORD-KPI-009", "customer.fatma",  "chef.layla",  295.00m, "Card", "Delivered",      4),
            new("ORD-KPI-010", "customer.omar",   "chef.mona",   240.00m, "Cash", "Delivered",      5),
            new("ORD-KPI-011", "customer.sara",   "chef.tarek",  480.00m, "Card", "Delivered",      6),

            // Prior 7 days (days 7..13 ago — baseline for 7d growth comparison)
            new("ORD-KPI-012", "customer.mariam", "chef.hassan", 520.00m, "Card", "Delivered",      8),
            new("ORD-KPI-013", "customer.ahmed",  "chef.layla",  340.00m, "Cash", "Delivered",      9),
            new("ORD-KPI-014", "customer.fatma",  "chef.tarek",  690.00m, "Card", "Delivered",     11),
            new("ORD-KPI-015", "customer.omar",   "chef.mona",   410.00m, "Card", "Delivered",     12),

            // Previous month (days 25..42 ago — baseline for month-over-month growth comparison)
            new("ORD-KPI-016", "customer.sara",   "chef.hassan", 890.00m, "Card", "Delivered",     28),
            new("ORD-KPI-017", "customer.mariam", "chef.layla",  740.00m, "Cash", "Delivered",     33),
            new("ORD-KPI-018", "customer.ahmed",  "chef.tarek",  960.00m, "Card", "Delivered",     38),

            // Earlier months in current year (for Year-to-Date & All-Time monthly chart buckets)
            new("ORD-KPI-019", "customer.fatma",  "chef.hassan", 1250.00m, "Card", "Delivered",    65),
            new("ORD-KPI-020", "customer.omar",   "chef.layla",  1120.00m, "Cash", "Delivered",    95),
            new("ORD-KPI-021", "customer.sara",   "chef.tarek",  1480.00m, "Card", "Delivered",   125),
            new("ORD-KPI-022", "customer.mariam", "chef.mona",    980.00m, "Card", "Delivered",   155)
        };

        var existingRefs = await context.DbContext.Orders
            .AsNoTracking()
            .Where(o => o.Reference.StartsWith("ORD-KPI-"))
            .Select(o => o.Reference)
            .ToHashSetAsync(context.CancellationToken);

        int seededCount = 0;
        var nowUtc = DateTime.UtcNow;

        foreach (var item in seeds)
        {
            if (existingRefs.Contains(item.Reference))
            {
                continue;
            }

            var customer = await context.UserManager.FindByEmailAsync($"{item.CustomerEmailPrefix}@{domain}");
            var chefUser = await context.UserManager.FindByEmailAsync($"{item.ChefEmailPrefix}@{domain}");
            if (customer is null || chefUser is null)
            {
                continue;
            }

            var chefProfile = await context.DbContext.ChefProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == chefUser.Id, context.CancellationToken);
            if (chefProfile is null)
            {
                continue;
            }

            decimal commissionRate = chefProfile.CommissionRate ?? 0.1500m;
            decimal commissionAmount = Math.Round(item.Total * commissionRate, 2);

            var order = new Models.Orders.Order
            {
                UserId = customer.Id,
                ChefProfileId = chefProfile.Id,
                Status = item.Status,
                PaymentMethod = item.PaymentMethod,
                PaymentStatus = item.Status == "Delivered" || item.PaymentMethod == "Card" ? "Paid" : "Pending",
                Reference = item.Reference,
                SubmissionToken = $"tok-{item.Reference.ToLowerInvariant()}",
                Subtotal = item.Total,
                PointsDiscount = 0.00m,
                Total = item.Total,
                CommissionRate = commissionRate,
                CommissionAmount = commissionAmount,
                DeliveryAddress = customer.Address ?? "Cairo, Egypt",
                CreatedAt = nowUtc.AddDays(-item.DaysAgo).AddHours(-3)
            };

            context.DbContext.Orders.Add(order);
            seededCount++;
        }

        if (seededCount > 0)
        {
            await context.DbContext.SaveChangesAsync(context.CancellationToken);
            context.Logger.LogInformation("Seeded {Count} demo KPI orders across multiple date ranges.", seededCount);
        }
    }
}
