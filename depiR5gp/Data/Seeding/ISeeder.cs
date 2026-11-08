namespace depiR5gp.Data.Seeding;

/// <summary>
/// Contract for database seeders. Seeders MUST be idempotent.
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Execution tier (Core runs first, then Catalog, then Demo).
    /// </summary>
    SeedTier Tier { get; }

    /// <summary>
    /// Relative execution order within the tier (ascending).
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the seeding logic against the provided context.
    /// </summary>
    Task SeedAsync(SeedContext context);
}

