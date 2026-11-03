using System.Security.Claims;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels.AdminUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace depiR5gp.Controllers;

// Customer & chef account management (SCRUM-358). Routes: /Admin/Users and /Admin/Users/ChangeStatus
[Authorize(Roles = AppRoles.Admin)]
[Route("Admin/Users")]
public class AdminUsersController(IUserManagementService userManagementService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] UserListFilterVM filter)
    {
        var model = await userManagementService.GetUsersAsync(filter);
        return View("~/Views/Admin/Users.cshtml", model);
    }

    [HttpPost("ChangeStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeUserStatusVM model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Invalid request.";
            return RedirectBack(model.ReturnUrl);
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int adminId))
        {
            return Forbid();
        }

        var result = await userManagementService.ChangeStatusAsync(adminId, model);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        return RedirectBack(model.ReturnUrl);
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
