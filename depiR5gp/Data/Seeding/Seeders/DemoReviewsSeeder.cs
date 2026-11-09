using depiR5gp.Models.Orders;
using depiR5gp.Models.ReviewsLoyalty;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds realistic demo customer reviews across Pending, Approved, and Rejected moderation statuses
/// along with their backing completed orders so the Admin Review Moderation tab can be tested out of the box.
/// </summary>
public class DemoReviewsSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Demo;
    public int Order => 30; // Runs after DemoUsersSeeder (Order = 10)

    private sealed record DemoReviewSeed(
        string OrderReference,
        string CustomerEmailPrefix,
        string ChefEmailPrefix,
        decimal OrderTotal,
        byte Rating,
        string ModerationStatus,
        string Comment,
        int DaysAgo);

    public async Task SeedAsync(SeedContext context)
    {
        var domain = context.Options.DemoEmailDomain.TrimStart('@');

        var seeds = new List<DemoReviewSeed>
        {
            new(
                "ORD-DEMO-REV-001",
                "customer.sara",
                "chef.hassan",
                340.00m,
                5,
                "Pending",
                "Superb okra with lamb meat tagine! Packaged perfectly hot and sealed.",
                1),
            new(
                "ORD-DEMO-REV-002",
                "customer.ahmed",
                "chef.layla",
                260.00m,
                4,
                "Pending",
                "The pistachio kunafa rolls were crispy and not overly sweet. Delivery took slightly longer than expected.",
                2),
            new(
                "ORD-DEMO-REV-003",
                "customer.omar",
                "chef.tarek",
                480.00m,
                2,
                "Pending",
                "Fish was fresh, but the sayadeya rice arrived lukewarm and was missing the tahini side.",
                3),
            new(
                "ORD-DEMO-REV-004",
                "customer.mariam",
                "chef.hassan",
                420.00m,
                5,
                "Approved",
                "The duck was browned to perfection and the molokhia was piping hot. Best homemade dinner in Maadi!",
                5),
            new(
                "ORD-DEMO-REV-005",
                "customer.fatma",
                "chef.layla",
                310.00m,
                5,
                "Approved",
                "Authentic baladi ghee flavor in the feteer. Portion was generous for 4 people.",
                7),
            new(
                "ORD-DEMO-REV-006",
                "customer.sara",
                "chef.mona",
                195.00m,
                4,
                "Approved",
                "Loved the roasted chickpea and quinoa bowl. Fresh ingredients and vibrant tahini dressing.",
                9),
            new(
                "ORD-DEMO-REV-007",
                "customer.ahmed",
                "chef.tarek",
                520.00m,
                1,
                "Rejected",
                "Unacceptable delay and completely wrong order delivered! Never ordering again.",
                12)
        };

        bool anyReviewsExist = await context.DbContext.Reviews.AnyAsync(context.CancellationToken);
        int seededCount = 0;

        foreach (var item in seeds)
        {
            var customerEmail = $"{item.CustomerEmailPrefix}@{domain}";
            var chefEmail = $"{item.ChefEmailPrefix}@{domain}";

            var customer = await context.UserManager.FindByEmailAsync(customerEmail);
            var chefUser = await context.UserManager.FindByEmailAsync(chefEmail);
            if (customer is null || chefUser is null)
            {
                continue;
            }

            var chefProfile = await context.DbContext.ChefProfiles
                .FirstOrDefaultAsync(c => c.UserId == chefUser.Id, context.CancellationToken);
            if (chefProfile is null)
            {
                continue;
            }

            var existingOrder = await context.DbContext.Orders
                .FirstOrDefaultAsync(o => o.Reference == item.OrderReference, context.CancellationToken);

            var createdAt = DateTime.UtcNow.AddDays(-item.DaysAgo);

            if (existingOrder is null)
            {
                var commissionRate = chefProfile.CommissionRate ?? 0.1500m;
                var commissionAmount = Math.Round(item.OrderTotal * commissionRate, 2);

                existingOrder = new Models.Orders.Order
                {
                    UserId = customer.Id,
                    ChefProfileId = chefProfile.Id,
                    Status = "Delivered",
                    PaymentMethod = "Cash",
                    PaymentStatus = "Paid",
                    Reference = item.OrderReference,
                    SubmissionToken = $"tok-{item.OrderReference.ToLowerInvariant()}",
                    Subtotal = item.OrderTotal,
                    PointsDiscount = 0.00m,
                    Total = item.OrderTotal,
                    CommissionRate = commissionRate,
                    CommissionAmount = commissionAmount,
                    DeliveryAddress = customer.Address ?? "Cairo, Egypt",
                    CreatedAt = createdAt.AddHours(-2)
                };

                context.DbContext.Orders.Add(existingOrder);
                await context.DbContext.SaveChangesAsync(context.CancellationToken);

                var review = new Review
                {
                    OrderId = existingOrder.Id,
                    UserId = customer.Id,
                    ChefProfileId = chefProfile.Id,
                    Rating = item.Rating,
                    ModerationStatus = item.ModerationStatus,
                    Comment = item.Comment,
                    CreatedAt = createdAt
                };

                context.DbContext.Reviews.Add(review);
                seededCount++;
            }
            else if (!anyReviewsExist)
            {
                bool hasReview = await context.DbContext.Reviews
                    .AnyAsync(r => r.OrderId == existingOrder.Id, context.CancellationToken);

                if (!hasReview)
                {
                    context.DbContext.Reviews.Add(new Review
                    {
                        OrderId = existingOrder.Id,
                        UserId = customer.Id,
                        ChefProfileId = chefProfile.Id,
                        Rating = item.Rating,
                        ModerationStatus = item.ModerationStatus,
                        Comment = item.Comment,
                        CreatedAt = createdAt
                    });
                    seededCount++;
                }
            }
        }

        if (seededCount > 0)
        {
            await context.DbContext.SaveChangesAsync(context.CancellationToken);
            context.Logger.LogInformation("Seeded {Count} demo reviews for moderation.", seededCount);
        }
    }
}
