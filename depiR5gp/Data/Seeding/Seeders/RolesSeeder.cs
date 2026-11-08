using depiR5gp.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds essential system Identity roles (Admin, Customer, Chef, Support).
/// </summary>
public class RolesSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Core;
    public int Order => 1;

    public async Task SeedAsync(SeedContext context)
    {
        foreach (var roleName in AppRoles.All)
        {
            if (!await context.RoleManager.RoleExistsAsync(roleName))
            {
                var result = await context.RoleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (result.Succeeded)
                {
                    context.Logger.LogInformation("Seeded role: {Role}", roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    context.Logger.LogWarning("Failed to seed role {Role}: {Errors}", roleName, errors);
                }
            }
        }
    }
}

