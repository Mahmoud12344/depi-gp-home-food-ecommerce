using depiR5gp.Models.Platform;
using depiR5gp.Models.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds the initial platform settings and baseline AI knowledge-base FAQs.
/// </summary>
public class PlatformSettingsSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Core;
    public int Order => 2;

    public async Task SeedAsync(SeedContext context)
    {
        if (!await context.DbContext.PlatformSettings.AnyAsync(context.CancellationToken))
        {
            context.DbContext.PlatformSettings.Add(new PlatformSettings
            {
                UpdatedByUserId = null,
                CommissionRate = 0.12m,
                RefundWindowHours = 24,
                UpdatedAt = DateTime.UtcNow
            });
            await context.DbContext.SaveChangesAsync(context.CancellationToken);
            context.Logger.LogInformation("Seeded platform settings with a 12% commission rate.");
        }

        if (!await context.DbContext.Faqs.AnyAsync(context.CancellationToken))
        {
            var now = DateTime.UtcNow;
            context.DbContext.Faqs.AddRange(
                new Faq
                {
                    Question = "[Ordering & Pickup] How does domestic kitchen counter pickup work?",
                    Answer = "Customers place an order online and receive the neighborhood pickup zone. Upon confirmation, the chef shares the exact pickup address and target pickup window for hot handover.",
                    UpdatedAt = now
                },
                new Faq
                {
                    Question = "[Hygiene & Safety] What hygiene standards do domestic kitchens follow?",
                    Answer = "All registered domestic kitchens undergo verification of national ID and medical food handler certificates before their menus are visible on the platform.",
                    UpdatedAt = now
                },
                new Faq
                {
                    Question = "[Payments & Refunds] Can I cancel an order and get a 100% refund?",
                    Answer = "Yes. Orders cancelled while still in Pending status (before kitchen preparation starts) receive a 100% full refund with zero cancellation fees.",
                    UpdatedAt = now
                });
            await context.DbContext.SaveChangesAsync(context.CancellationToken);
            context.Logger.LogInformation("Seeded baseline platform FAQ entries.");
        }
    }
}
