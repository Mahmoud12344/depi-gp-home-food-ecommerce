namespace depiR5gp.Data.Seeding;

/// <summary>
/// Execution tier for database seeders to maintain strict dependency order.
/// </summary>
public enum SeedTier
{
    /// <summary>
    /// Critical system infrastructure (e.g., Roles, System Settings, SuperAdmin).
    /// </summary>
    Core = 1,

    /// <summary>
    /// Reference/lookup catalog data (e.g., Categories, Cuisines, Cities).
    /// </summary>
    Catalog = 2,

    /// <summary>
    /// Realistic sample/demo records for local development, QA, and automated test fixtures.
    /// </summary>
    Demo = 3
}

