using System.Security.Claims;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Controllers;

public class ChefsController(
    IChefService chefService,
    IUnitOfWork unitOfWork,
    ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(ChefFilterViewModel filter)
    => View(await chefService.GetCatalogAsync(filter));

    public IActionResult Details(int id) => View();
    

    [HttpGet]
    // [Authorize(Roles = AppRoles.Chef)]
    public async Task<IActionResult> Onboarding()
    {
        int userId = GetCurrentUserId();

        if (await chefService.HasPendingOrApprovedApplicationAsync(userId))
        {
            TempData["ErrorMessage"] = "You already have a pending application or your chef account is already activated.";
            return RedirectToAction("Index", "Home");
        }

       decimal CommissionRate=  chefService.GetCommissionRate().Result;
       ViewBag.CommissionRate = CommissionRate;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboarding(ChefApplicationViewModel model)
    {
        if (!model.HasAcceptedTerms)
        {
            ModelState.AddModelError(
                nameof(model.HasAcceptedTerms),
                "You must accept the commission agreement terms."
            );
        }

        if (!PickupAreas.IsValid(model.Area))
        {
            ModelState.AddModelError(
                nameof(model.Area),
                "المنطقة غير صحيحة أو غير موجودة في القائمة."
            );
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