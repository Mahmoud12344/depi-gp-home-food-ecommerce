using System.Security.Claims;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // استدعاء الدالة اللي عملناها في الـ Service
        var model = await chefService.GetChefProfileForEditAsync(userId);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Please submit your chef onboarding application first.";
            return RedirectToAction(nameof(Onboarding));
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Chef)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditChefProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int userId = GetCurrentUserId();

        // استدعاء دالة الحفظ من الـ Service
        bool isUpdated = await chefService.UpdateChefProfileAsync(userId, model);

        if (!isUpdated)
        {
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ التعديلات.");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تحديث بياناتك بنجاح!";

        // إعادة توجيه الشيف لصفحة التعديل لرؤية التغييرات (أو يمكنك توجيهه لصفحة البروفايل)
        return RedirectToAction(nameof(EditProfile));
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