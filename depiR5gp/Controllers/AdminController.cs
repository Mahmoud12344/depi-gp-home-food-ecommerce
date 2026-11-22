using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using System.Threading.Tasks;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private static readonly string[] ValidReviewStatuses = ["Pending", "Approved", "Rejected"];
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminService _adminService;

    public AdminController(IUnitOfWork unitOfWork, IAdminService adminService)
    {
        _unitOfWork = unitOfWork;
        _adminService = adminService;
    }

    public IActionResult Index() => View();
    public IActionResult Chefs() => View();
    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();
    public IActionResult Settings() => View();

    [HttpGet]
    public async Task<IActionResult> Reviews(string? status = "all")
    {
        string normalizedStatus = ValidReviewStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        var reviews = await _unitOfWork.Reviews.GetForModerationAsync(normalizedStatus);
        ViewBag.CurrentStatus = normalizedStatus;

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(int reviewId, string moderationAction, string? status = "all")
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review is not null)
        {
            switch (moderationAction)
            {
                case "Approve":
                    review.ModerationStatus = "Approved";
                    await _unitOfWork.SaveChangesAsync();
                    break;
                case "Reject":
                    review.ModerationStatus = "Rejected";
                    await _unitOfWork.SaveChangesAsync();
                    break;
                case "Delete":
                    _unitOfWork.Reviews.Remove(review);
                    await _unitOfWork.SaveChangesAsync();
                    break;
            }
        }

        string redirectStatus = ValidReviewStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        return RedirectToAction(nameof(Reviews), new { status = redirectStatus });
    }

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
