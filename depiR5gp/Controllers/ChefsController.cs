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

        bool chefExists = await context.ChefProfiles.AnyAsync(c =>
            c.Id == id && c.Status == "Approved" && c.User.Status == UserStatus.Active);
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
        if (!TryGetCurrentUserId(out int userId)) return Challenge();

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
        if (!TryGetCurrentUserId(out int userId)) return Challenge();

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

    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    [HttpGet]
    [Authorize(Roles = AppRoles.Chef)]
    public async Task<IActionResult> EditProfile(string? tab = null)
    {
        if (!TryGetCurrentUserId(out int userId)) return Challenge();

        var statusCheck = await chefService.GetChefApplicationStatusAsync(userId);
        if (statusCheck is null)
        {
            TempData["ErrorMessage"] = "Please submit your chef onboarding application first.";
            return RedirectToAction(nameof(Onboarding));
        }
        if (statusCheck.Status != "Approved")
        {
            return RedirectToAction("Index", "Dashboard");
        }

        var model = await chefService.GetChefProfileForEditAsync(userId);
        if (model == null)
        {
            return RedirectToAction(nameof(Onboarding));
        }

        model.ActiveSection = tab?.ToLowerInvariant() is "hours" or "documents" ? tab.ToLowerInvariant() : "profile";
        await PopulateChefProfileSectionsAsync(userId, model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Chef)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditChefProfileViewModel model)
    {
        if (!TryGetCurrentUserId(out int userId)) return Challenge();

        var statusCheck = await chefService.GetChefApplicationStatusAsync(userId);
        if (statusCheck is null) return RedirectToAction(nameof(Onboarding));
        if (statusCheck.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        if (!string.IsNullOrWhiteSpace(model.Area) && !PickupAreas.IsValid(model.Area))
        {
            ModelState.AddModelError(nameof(model.Area), "Please select a valid pickup area from the list.");
        }

        if (!ModelState.IsValid)
        {
            model.ActiveSection = "profile";
            await PopulateChefProfileSectionsAsync(userId, model);
            return View(model);
        }

        bool isUpdated = await chefService.UpdateChefProfileAsync(userId, model);

        if (!isUpdated)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving your profile changes.");
            model.ActiveSection = "profile";
            await PopulateChefProfileSectionsAsync(userId, model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Kitchen profile updated successfully!";
        return RedirectToAction(nameof(EditProfile), new { tab = "profile" });
    }

    private async Task PopulateChefProfileSectionsAsync(int userId, EditChefProfileViewModel model)
    {
        var chef = await context.ChefProfiles
            .AsNoTracking()
            .Include(c => c.WorkingHours)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (chef is null) return;

        ViewData["ChefHeader"] = await depiR5gp.ViewModels.ChefPortalHeaderVM.LoadAsync(context, chef);

        var existingByDay = chef.WorkingHours.ToDictionary(w => w.DayOfWeek);
        model.WorkingHoursSchedule = new depiR5gp.ViewModels.ChefWorkingHoursVM
        {
            Days = Enumerable.Range(0, 7).Select(i =>
            {
                byte day = (byte)i;
                if (existingByDay.TryGetValue(day, out var hours))
                {
                    return new depiR5gp.ViewModels.ChefWorkingDayVM
                    {
                        DayOfWeek = day,
                        DayName = DayNames[i],
                        IsOpen = true,
                        OpenTime = hours.OpenTime.ToString("HH:mm"),
                        CloseTime = hours.CloseTime.ToString("HH:mm")
                    };
                }

                return new depiR5gp.ViewModels.ChefWorkingDayVM
                {
                    DayOfWeek = day,
                    DayName = DayNames[i],
                    IsOpen = false,
                    OpenTime = "12:00",
                    CloseTime = "22:00"
                };
            }).ToList()
        };

        model.DocumentsSection = new depiR5gp.ViewModels.ChefDocumentsVM
        {
            KitchenName = chef.KitchenName,
            Status = chef.Status,
            Documents = chef.Documents.OrderByDescending(d => d.UploadedAt).ToList()
        };
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}