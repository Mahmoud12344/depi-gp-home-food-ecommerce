using depiR5gp.Models.Users;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Optional first-admin bootstrap for clean deployments. Runs only when BOTH
/// Seeding:AdminEmail and Seeding:AdminPassword are configured (e.g. Azure App Settings
/// "Seeding__AdminEmail" / "Seeding__AdminPassword"). Never overwrites an existing user.
/// </summary>
public sealed class AdminBootstrapSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Core;
    public int Order => 5;

    public async Task SeedAsync(SeedContext context)
    {
        string? email = context.Configuration["Seeding:AdminEmail"];
        string? password = context.Configuration["Seeding:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (await context.UserManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = "Platform",
            LastName = "Admin",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var create = await context.UserManager.CreateAsync(admin, password);
        if (!create.Succeeded)
        {
            context.Logger.LogWarning("Admin bootstrap failed: {Errors}", string.Join(", ", create.Errors.Select(e => e.Description)));
            return;
        }

        await context.UserManager.AddToRoleAsync(admin, AppRoles.Admin);
        context.Logger.LogInformation("Bootstrap admin {Email} created.", email);
    }
}
