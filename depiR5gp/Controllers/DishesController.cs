using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Menu;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

public class DishesController(ApplicationDbContext context, UserManager<AppUser> userManager, ILogger<DishesController> logger, IDishService dishService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DishFilterViewModel filter)
        => View(await dishService.GetCatalogAsync(filter));

    [HttpGet("Dishes/Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        Dish? dish = await context.Dishes
            .AsNoTracking()
            .Include(d => d.DishOptions)
            .Include(d => d.Category)
            .Include(d => d.Cuisine)
            .Include(d => d.ChefProfile)
                .ThenInclude(c => c.User)
            .SingleOrDefaultAsync(d => d.Id == id && !d.IsArchived);

        if (dish is null)
        {
            logger.LogWarning(
                "Dish details not found. DishId={DishId}, Database={Database}, Server={Server}",
                id,
                context.Database.GetDbConnection().Database,
                context.Database.GetDbConnection().DataSource);
            return NotFound();
        }

        List<Review> reviews = await context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ChefProfileId == dish.ChefProfileId && r.ModerationStatus == "Approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        double averageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => (double)r.Rating), 1) : 0;

        // Build dynamic gallery starting with this dish's photo, then other photos from the same kitchen
        List<string> galleryImages = [];
        if (!string.IsNullOrWhiteSpace(dish.ImageUrl))
        {
            galleryImages.Add(dish.ImageUrl);
        }

        List<string?> siblingDishImages = await context.Dishes
            .AsNoTracking()
            .Where(d => d.ChefProfileId == dish.ChefProfileId && d.Id != dish.Id && !d.IsArchived && d.ImageUrl != null)
            .Select(d => d.ImageUrl)
            .Take(4)
            .ToListAsync();

        foreach (var img in siblingDishImages)
        {
            if (!string.IsNullOrWhiteSpace(img) && !galleryImages.Contains(img))
            {
                galleryImages.Add(img);
            }
        }

        if (!string.IsNullOrWhiteSpace(dish.ChefProfile?.PhotoUrl) && !galleryImages.Contains(dish.ChefProfile.PhotoUrl))
        {
            galleryImages.Add(dish.ChefProfile.PhotoUrl);
        }

        if (galleryImages.Count == 0)
        {
            galleryImages.Add("/icons/chibi-chef.png");
        }

        AppUser? user = await userManager.GetUserAsync(User);
        bool isFavorite = user is not null && await context.Favorites.AnyAsync(favorite => favorite.UserId == user.Id && favorite.DishId == id);

        return View(new DishDetailsVM
        {
            Dish = dish,
            IsFavorite = isFavorite,
            GalleryImages = galleryImages,
            Reviews = reviews,
            AverageRating = averageRating,
            ReviewCount = reviews.Count
        });
    }

    [HttpGet]
    public async Task<IActionResult> Catalog()
    {
        AppUser? user = await userManager.GetUserAsync(User);
        HashSet<int> favoriteDishIds = user is null
            ? []
            : (await context.Favorites.Where(favorite => favorite.UserId == user.Id && favorite.DishId != null).Select(favorite => favorite.DishId!.Value).ToListAsync()).ToHashSet();

        var dishes = await context.Dishes
            .AsNoTracking()
            .Where(dish => !dish.IsArchived)
            .Select(dish => new
            {
                id = dish.Id,
                name = dish.Name,
                price = dish.Price,
                image = dish.ImageUrl,
                isAvailable = dish.IsAvailable,
                isFavorite = favoriteDishIds.Contains(dish.Id)
            })
            .ToListAsync();

        return Json(dishes);
    }

    [HttpGet]
    public async Task<IActionResult> FavoriteState(int id)
    {
        AppUser? user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized(new { loginRequired = true });

        bool isFavorite = await context.Favorites.AnyAsync(favorite => favorite.UserId == user.Id && favorite.DishId == id);
        return Json(new { isFavorite });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized(new { loginRequired = true });

        var dishExists = await context.Dishes
            .AnyAsync(d => d.Id == id && !d.IsArchived);

        if (!dishExists)
            return NotFound(new
            {
                success = false,
                message = "This dish is no longer available."
            });

        var favorite = await context.Favorites
            .SingleOrDefaultAsync(f =>
                f.UserId == user.Id &&
                f.DishId == id);

        bool isFavorite;

        if (favorite == null)
        {
            context.Favorites.Add(new Favorite
            {
                UserId = user.Id,
                DishId = id
            });

            isFavorite = true;
        }
        else
        {
            context.Favorites.Remove(favorite);
            isFavorite = false;
        }

        await context.SaveChangesAsync();

        var count = await context.Favorites
            .CountAsync(f => f.UserId == user.Id);

        logger.LogInformation(
            "Favorite toggled. UserId={UserId}, DishId={DishId}, IsFavorite={IsFavorite}, Count={Count}",
            user.Id,
            id,
            isFavorite,
            count
        );

        return Json(new
        {
            success = true,
            isFavorite,
            count
        });
    }

    [HttpGet]
    public async Task<IActionResult> FavoritesCount()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Json(new { count = 0 });

        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Json(new { count = 0 });

        var count = await context.Favorites
            .CountAsync(f => f.UserId == user.Id);

        return Json(new { count });
    }
}