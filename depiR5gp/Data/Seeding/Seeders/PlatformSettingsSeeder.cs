using depiR5gp.Models.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds the initial platform settings with a 12% commission rate.
/// </summary>
public class PlatformSettingsSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Core;
    public int Order => 2;

    public async Task SeedAsync(SeedContext context)
    {
        if (await context.DbContext.PlatformSettings.AnyAsync(context.CancellationToken))
        {
            return;
        }

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
}
