using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using System.Threading.Tasks;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IActionResult Index() => View();
    public IActionResult Chefs() => View();
    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();
    public IActionResult Settings() => View();

    [HttpGet]
    public async Task<IActionResult> ReviewApplication(int id)
    {
        var chefProfile = await _adminService.GetChefApplicationAsync(id);
        if (chefProfile == null) return NotFound("Chef application not found.");
        return View(chefProfile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveApplication(int id, string approvalNotes)
    {
        var success = await _adminService.ApproveApplicationAsync(id, approvalNotes);
        if (!success) return NotFound();

        TempData["SuccessMessage"] = "Chef application has been approved successfully.";
        return RedirectToAction(nameof(Chefs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int id, string rejectionReason)
    {
        var success = await _adminService.RejectApplicationAsync(id, rejectionReason);
        if (!success) return NotFound();

        TempData["SuccessMessage"] = "Chef application has been rejected.";
        return RedirectToAction(nameof(Chefs));
    }
}