using depiR5gp.Data;
using depiR5gp.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace depiR5gp.Data.Seeding;

/// <summary>
/// Orchestrates discovery and sequential execution of all registered ISeeder instances.
/// </summary>
public class DatabaseSeeder
{
    private readonly IEnumerable<ISeeder> _seeders;
    private readonly IOptions<SeedingOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IEnumerable<ISeeder> seeders,
        IOptions<SeedingOptions> options,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _seeders = seeders;
        _options = options;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            _logger.LogInformation("Database seeding is disabled in configuration.");
            return;
        }

        var db = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = _serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        var context = new SeedContext
        {
            ServiceProvider = _serviceProvider,
            DbContext = db,
            UserManager = userManager,
            RoleManager = roleManager,
            Logger = _logger,
            Configuration = _configuration,
            Options = options,
            CancellationToken = cancellationToken
        };

        var orderedSeeders = _seeders
            .OrderBy(s => (int)s.Tier)
            .ThenBy(s => s.Order)
            .ToList();

        _logger.LogInformation("Starting database seeding ({Count} registered seeders)...", orderedSeeders.Count);

        foreach (var seeder in orderedSeeders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (seeder.Tier == SeedTier.Demo && !options.EnableDemoData)
            {
                _logger.LogDebug("Skipping demo seeder {SeederName} because EnableDemoData is false.", seeder.GetType().Name);
                continue;
            }

            var seederName = seeder.GetType().Name;
            _logger.LogInformation("Executing seeder: {SeederName} (Tier: {Tier}, Order: {Order})...", seederName, seeder.Tier, seeder.Order);

            try
            {
                await seeder.SeedAsync(context);
                _logger.LogInformation("Completed seeder: {SeederName}.", seederName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed executing seeder {SeederName}.", seederName);
                throw;
            }
        }

        _logger.LogInformation("Database seeding finished successfully.");
    }
}

