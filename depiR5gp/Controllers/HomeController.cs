using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Controllers;

public class HomeController(IChefService chefService, IDishService dishService) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppRoles.Chef))
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) && userId > 0)
            {
                var chefProfile = await chefService.GetChefApplicationStatusAsync(userId);
                if (chefProfile is null) return RedirectToAction("Onboarding", "Chefs");
                if (chefProfile.Status != "Approved") return RedirectToAction("Index", "Dashboard");
            }
            return RedirectToAction("Index", "ChefDashboard");
        }

        try
        {
            var catalog = await dishService.GetCatalogAsync(new Models.Menu.DishFilterViewModel());
            if (catalog.Dishes.Count > 0)
            {
                ViewBag.CarouselDishIds = Enumerable.Range(0, 6)
                    .Select(i => catalog.Dishes[i % catalog.Dishes.Count].Id)
                    .ToArray();
            }
        }
        catch
        {
            // Keep default carousel fallback IDs if database is unavailable
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
