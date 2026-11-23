using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models;
using depiR5gp.Models.Users;

namespace depiR5gp.Controllers;

public class HomeController : Controller {
    public IActionResult Index() {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppRoles.Chef))
        {
            return RedirectToAction("Index", "ChefDashboard");
        }
        return View();
    }

    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}