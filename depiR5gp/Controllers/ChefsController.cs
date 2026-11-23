using System.Security.Claims;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Controllers;

public class ChefsController(
    IChefService chefService,
    IUnitOfWork unitOfWork,
    ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(ChefFilterViewModel filter)
    => View(await chefService.GetCatalogAsync(filter));

    public IActionResult Details(int id) => RedirectToAction("Details", "ChefProfile", new { id });

    [HttpGet]
    public async Task<IActionResult> FavoriteIds()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { chefIds = Array.Empty<int>() });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            return Json(new { chefIds = Array.Empty<int>() });
        }

        var chefIds = await context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.ChefProfileId != null)
            .Select(f => f.ChefProfileId!.Value)
            .ToListAsync();

        return Json(new { chefIds });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            return Unauthorized(new { loginRequired = true });
        }

        bool chefExists = await context.ChefProfiles.AnyAsync(c => c.Id == id && c.Status == "Approved");
        if (!chefExists)
        {
            return NotFound(new { success = false, message = "This chef kitchen is no longer available." });
        }

        var favorite = await context.Favorites
            .SingleOrDefaultAsync(f => f.UserId == userId && f.ChefProfileId == id);

        bool isFavorite;
        if (favorite is null)
        {
            context.Favorites.Add(new Favorite { UserId = userId, ChefProfileId = id });
            isFavorite = true;
        }
        else
        {
            context.Favorites.Remove(favorite);
            isFavorite = false;
        }

        await context.SaveChangesAsync();

        int count = await context.Favorites.CountAsync(f => f.UserId == userId);
        return Json(new { success = true, isFavorite, count });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Chef)]
    public async Task<IActionResult> Onboarding()
    {
        int userId = GetCurrentUserId();

        if (await chefService.HasPendingOrApprovedApplicationAsync(userId))
        {
            TempData["ErrorMessage"] = "You already have a pending application or your chef account is already activated.";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.CommissionRate = await chefService.GetCommissionRate();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Chef)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboarding(ChefApplicationViewModel model)
    {
        // [SCRUM-345]   التحقق من موافقة الشيف على اتفاقية العمولة
        if (!model.HasAcceptedTerms)
        {
            ModelState.AddModelError(
                nameof(model.HasAcceptedTerms),
                "You must accept the Commission Agreement to submit your application."
            );
        }

        if (!PickupAreas.IsValid(model.Area))
        {
            ModelState.AddModelError(
                nameof(model.Area),
                "Please select a valid pickup area from the list."
            );
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CommissionRate = await chefService.GetCommissionRate();
            return View(model);
        }

        int userId = GetCurrentUserId();

        // إرسال البيانات للـ Service لمعالجتها
        var result = await chefService.SubmitApplicationAsync(userId, model);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.CommissionRate = await chefService.GetCommissionRate();
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Index", "Dashboard");
    }

    // ==========================================
    //  تعديل الملف الشخصي للشيف (Edit Profile)
    // ==========================================

    [HttpGet]
    [Authorize(Roles = AppRoles.Chef)]
    public async Task<IActionResult> EditProfile()
    {
        int userId = GetCurrentUserId();

        var model = await chefService.GetChefProfileForEditAsync(userId);
        if (model == null)
        {
            TempData["ErrorMessage"] = "Please submit your chef onboarding application first.";
            return RedirectToAction(nameof(Onboarding));
        }

        await PopulateChefHeaderAsync(userId);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Chef)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditChefProfileViewModel model)
    {
        int userId = GetCurrentUserId();

        if (!string.IsNullOrWhiteSpace(model.Area) && !PickupAreas.IsValid(model.Area))
        {
            ModelState.AddModelError(nameof(model.Area), "Please select a valid pickup area from the list.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateChefHeaderAsync(userId);
            return View(model);
        }

        bool isUpdated = await chefService.UpdateChefProfileAsync(userId, model);

        if (!isUpdated)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving your profile changes.");
            await PopulateChefHeaderAsync(userId);
            return View(model);
        }

        TempData["SuccessMessage"] = "Kitchen profile updated successfully!";
        return RedirectToAction(nameof(EditProfile));
    }

    private async Task PopulateChefHeaderAsync(int userId)
    {
        var chef = await context.ChefProfiles.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
        if (chef is not null)
        {
            ViewData["ChefHeader"] = await depiR5gp.ViewModels.ChefPortalHeaderVM.LoadAsync(context, chef);
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out int userId) && userId > 0)
        {
            return userId;
        }

        var existingUser = context.Users.FirstOrDefault();
        if (existingUser != null)
        {
            return existingUser.Id;
        }

        var dummyUser = new depiR5gp.Models.Users.AppUser
        {
            UserName = "testchef@example.com",
            Email = "testchef@example.com"
        };
        context.Users.Add(dummyUser);
        context.SaveChanges();

        return dummyUser.Id;
    }
}