using depiR5gp.Data.Seeding;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Data.Seeding.Seeders;

public sealed class DemoDishesSeeder : ISeeder
{
    public SeedTier Tier => SeedTier.Demo;
    public int Order => 20;

    private sealed record DishSeed(
        string Name,
        string Description,
        decimal Price,
        string ImageUrl,
        bool IsAvailable,
        string ChefEmailPrefix,
        string KitchenName,
        string CategoryName,
        string CuisineName);

    private static readonly DishSeed[] Dishes =
    [
        new("Molokhia with Roasted Baladi Chicken", "Slow-cooked molokhia with garlic, coriander, and roasted chicken served with rice.", 185m, "/icons/picnic-scene.png", true, "chef.hassan", "Hassan's Traditional Grill", "Main Courses", "Egyptian"),
        new("Creamy Mushroom Chicken Pasta", "Silky cream sauce, sautéed mushrooms, herbs, and tender chicken over handmade pasta.", 220m, "/icons/premium_photo-1700746098391-84d82ad82294.webp", true, "chef.mona", "Mona's Vegan Healthy Kitchen", "Main Courses", "International"),
        new("Alexandrian Sayadeya Fish", "Seasoned baked fish with fragrant tomato rice and fresh lemon-herb dressing.", 275m, "/icons/red-and-white-checkered-tablecloth-with-green-lush-foliage-at-background.png", true, "chef.tarek", "Tarek's Alexandria Seafood", "Seafood", "Alexandrian"),
        new("Pistachio Kunafa Rolls", "Crisp golden kunafa rolls filled with pistachio cream and finished with light syrup.", 135m, "/icons/craiyon_201037_Flat_vector_sticker__smiling_sushi_roll_character_with_salmon_and_rice_layers__on_a_solid_yellow_bac.png", true, "chef.layla", "Layla's Artisanal Patisserie", "Desserts", "Egyptian"),
        new("Smoked Beef Brisket Platter", "Tender slow-smoked brisket with house barbecue sauce, pickles, and roasted potatoes.", 340m, "/icons/red-and-white-checkered-tablecloth-with-lush-foliage-at-background.png", false, "chef.karim", "Karim's Slow-Cooked Smoked Meats", "Main Courses", "International")
    ];

    public async Task SeedAsync(SeedContext context)
    {
        foreach (DishSeed seed in Dishes)
        {
            if (await context.DbContext.Dishes.AnyAsync(dish => dish.Name == seed.Name, context.CancellationToken))
            {
                continue;
            }

            AppUser? chefUser = await context.UserManager.FindByEmailAsync($"{seed.ChefEmailPrefix}@{context.Options.DemoEmailDomain.TrimStart('@')}");
            if (chefUser is null)
            {
                context.Logger.LogWarning("Skipping dish seed {DishName}: chef user {EmailPrefix} was not found.", seed.Name, seed.ChefEmailPrefix);
                continue;
            }

            ChefProfile chef = await GetOrCreateChefAsync(context, seed, chefUser.Id);
            Category category = await GetOrCreateCategoryAsync(context, seed.CategoryName);
            Cuisine cuisine = await GetOrCreateCuisineAsync(context, seed.CuisineName);

            context.DbContext.Dishes.Add(new Dish
            {
                ChefProfileId = chef.Id,
                CategoryId = category.Id,
                CuisineId = cuisine.Id,
                Name = seed.Name,
                Description = seed.Description,
                Price = seed.Price,
                ImageUrl = seed.ImageUrl,
                IsAvailable = seed.IsAvailable,
                IsArchived = false
            });
        }

        await context.DbContext.SaveChangesAsync(context.CancellationToken);
        context.Logger.LogInformation("Ensured {DishCount} development dishes exist.", Dishes.Length);
    }

    private static async Task<ChefProfile> GetOrCreateChefAsync(SeedContext context, DishSeed seed, int userId)
    {
        ChefProfile? chef = await context.DbContext.ChefProfiles.SingleOrDefaultAsync(profile => profile.UserId == userId, context.CancellationToken);
        if (chef is not null)
        {
            return chef;
        }

        chef = new ChefProfile
        {
            UserId = userId,
            KitchenName = seed.KitchenName,
            Bio = $"Local development kitchen for {seed.KitchenName}.",
            Address = "Cairo, Egypt",
            Status = "Approved",
            HasAcceptedTerms = true
        };
        context.DbContext.ChefProfiles.Add(chef);
        await context.DbContext.SaveChangesAsync(context.CancellationToken);
        return chef;
    }

    private static async Task<Category> GetOrCreateCategoryAsync(SeedContext context, string name)
    {
        Category? category = await context.DbContext.Categories.SingleOrDefaultAsync(item => item.Name == name, context.CancellationToken);
        if (category is not null)
        {
            return category;
        }

        category = new Category { Name = name };
        context.DbContext.Categories.Add(category);
        await context.DbContext.SaveChangesAsync(context.CancellationToken);
        return category;
    }

    private static async Task<Cuisine> GetOrCreateCuisineAsync(SeedContext context, string name)
    {
        Cuisine? cuisine = await context.DbContext.Cuisines.SingleOrDefaultAsync(item => item.Name == name, context.CancellationToken);
        if (cuisine is not null)
        {
            return cuisine;
        }

        cuisine = new Cuisine { Name = name };
        context.DbContext.Cuisines.Add(cuisine);
        await context.DbContext.SaveChangesAsync(context.CancellationToken);
        return cuisine;
    }
}
