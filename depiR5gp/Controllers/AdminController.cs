using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController(
    IUnitOfWork unitOfWork,
    IAdminDashboardService dashboardService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? range = "month", CancellationToken cancellationToken = default)
    {
        var model = await dashboardService.GetDashboardAsync(range, cancellationToken);
        ViewData["PendingChefApplicationsCount"] = model.PendingApplicationsCount;
        return View(model);
    }

    public IActionResult Chefs() => View();
    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();
    public IActionResult Settings() => View();
}


