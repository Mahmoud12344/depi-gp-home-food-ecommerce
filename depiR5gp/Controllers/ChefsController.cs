using depiR5gp.Models.Chefs;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace depiR5gp.Controllers;

public class ChefsController(
    IChefService chefService,
    IUnitOfWork unitOfWork,
    ApplicationDbContext context) : Controller
{
    public IActionResult Index() => View();

    public IActionResult Details(int id) => View();

    [HttpGet]
    public async Task<IActionResult> Onboarding()
    {
        int userId = GetCurrentUserId();

        if (await chefService.HasPendingOrApprovedApplicationAsync(userId))
        {
            TempData["ErrorMessage"] = "You already have a pending application or your chef account is already activated.";
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboarding(ChefApplicationViewModel model)
    {
        // [SCRUM-345]   التحقق من موافقة الشيف على اتفاقية العمولة
        if (!model.HasAcceptedTerms)
        {
            ModelState.AddModelError("HasAcceptedTerms", "You must accept the Commission Agreement to submit your application.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int userId = GetCurrentUserId();

        // إرسال البيانات للـ Service لمعالجتها
        var result = await chefService.SubmitApplicationAsync(userId, model);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Index", "Home");
    }

    // ==========================================
    //  تعديل الملف الشخصي للشيف (Edit Profile)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        int userId = GetCurrentUserId();

        // استدعاء الدالة اللي عملناها في الـ Service
        var model = await chefService.GetChefProfileForEditAsync(userId);

        if (model == null)
        {
            return NotFound("لم يتم العثور على بروفايل الشيف أو لم يتم قبوله بعد.");
        }

        return View(model);
    }

    [HttpPost]
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