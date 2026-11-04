using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace depiR5gp.Data.Seeding;

/// <summary>
/// Service collection and host extensions for seeding.
/// </summary>
public static class SeedingExtensions
{
    /// <summary>
    /// Registers seeding configuration options, the orchestrator, and all concrete ISeeder implementations.
    /// </summary>
    public static IServiceCollection AddSeeders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedingOptions>(configuration.GetSection(SeedingOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        // Auto-register all concrete ISeeder classes in the current assembly
        var seederTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ISeeder).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in seederTypes)
        {
            services.AddScoped(typeof(ISeeder), type);
        }

        return services;
    }

    /// <summary>
    /// Executes all registered seeders within an isolated service scope.
    /// </summary>
    public static async Task SeedDatabaseAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAllAsync(cancellationToken);
    }
}

