using Microsoft.AspNetCore.Mvc;
using depiR5gp.Data;
using depiR5gp.Services.Interfaces;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using depiR5gp.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class DashboardController(
    IChefService chefService,
    ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Challenge();
        }

        var chefProfile = await chefService.GetChefApplicationStatusAsync(userId);
        if (chefProfile is null)
        {
            return RedirectToAction("Onboarding", "Chefs");
        }

        if (chefProfile.Status == "Approved")
        {
            ViewData["ChefHeader"] = await ChefPortalHeaderVM.LoadAsync(context, chefProfile);
        }

        var viewModel = new ChefApplicationStatusVM
        {
            Status = chefProfile.Status,
            RejectionNotes = chefProfile.RejectionReason
        };

        return View(viewModel);
    }
}
