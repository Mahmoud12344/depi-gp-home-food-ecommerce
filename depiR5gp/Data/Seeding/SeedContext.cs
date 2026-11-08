using depiR5gp.Data;
using depiR5gp.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding;

/// <summary>
/// Encapsulates services, options, and cancellation context provided to each ISeeder.
/// </summary>
public class SeedContext
{
    public required IServiceProvider ServiceProvider { get; init; }
    public required ApplicationDbContext DbContext { get; init; }
    public required UserManager<AppUser> UserManager { get; init; }
    public required RoleManager<IdentityRole<int>> RoleManager { get; init; }
    public required ILogger Logger { get; init; }
    public required IConfiguration Configuration { get; init; }
    public required SeedingOptions Options { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

