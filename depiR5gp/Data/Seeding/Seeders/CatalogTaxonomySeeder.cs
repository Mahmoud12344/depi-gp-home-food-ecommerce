using depiR5gp.Models.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace depiR5gp.Data.Seeding.Seeders;

/// <summary>
/// Seeds core menu taxonomy (Categories and Cuisines) across all environments so approved chefs
/// always have valid taxonomy options when adding dishes.
/// </summary>
public sealed class CatalogTaxonomySeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Core;
    public int Order => 3;

    private static readonly string[] CoreCategories =
    [
        "Main Courses",
        "Clay Tagines",
        "Seafood",
        "Artisan Bakery",
        "Desserts",
        "Healthy & Vegan"
    ];

    private static readonly string[] CoreCuisines =
    [
        "Egyptian",
        "Alexandrian",
        "Oriental Patisserie",
        "Mediterranean",
        "International"
    ];

    public async Task SeedAsync(SeedContext context)
    {
        var existingCategories = await context.DbContext.Categories
            .Select(c => c.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, context.CancellationToken);

        int addedCategories = 0;
        foreach (string name in CoreCategories)
        {
            if (!existingCategories.Contains(name))
            {
                context.DbContext.Categories.Add(new Category { Name = name });
                addedCategories++;
            }
        }

        var existingCuisines = await context.DbContext.Cuisines
            .Select(c => c.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, context.CancellationToken);

        int addedCuisines = 0;
        foreach (string name in CoreCuisines)
        {
            if (!existingCuisines.Contains(name))
            {
                context.DbContext.Cuisines.Add(new Cuisine { Name = name });
                addedCuisines++;
            }
        }

        if (addedCategories > 0 || addedCuisines > 0)
        {
            await context.DbContext.SaveChangesAsync(context.CancellationToken);
            context.Logger.LogInformation(
                "Seeded {CategoryCount} core categories and {CuisineCount} core cuisines.",
                addedCategories,
                addedCuisines);
        }
    }
}
