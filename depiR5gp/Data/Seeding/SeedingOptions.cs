namespace depiR5gp.Data.Seeding;

/// <summary>
/// Configuration settings for the database seeding pipeline.
/// </summary>
public class SeedingOptions
{
    public const string SectionName = "Seeding";

    /// <summary>
    /// Master toggle for all seeding.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to execute Tier 3 (Demo) seeders.
    /// </summary>
    public bool EnableDemoData { get; set; } = true;

    /// <summary>
    /// Default initial password for generated demo accounts.
    /// </summary>
    public string DefaultPassword { get; set; } = "Demo@12345";

    /// <summary>
    /// Domain suffix for generated demo emails (e.g., 'seed.homechef.test').
    /// </summary>
    public string DemoEmailDomain { get; set; } = "seed.homechef.test";
}

