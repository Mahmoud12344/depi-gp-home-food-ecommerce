using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Platform;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using System.Threading.Tasks;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private static readonly string[] ValidReviewStatuses = ["Pending", "Approved", "Rejected"];
    private static readonly string[] ValidApplicationStatuses = ["Pending", "Approved", "Rejected"];
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _dashboardService;

    public AdminController(
        IUnitOfWork unitOfWork,
        IAdminService adminService,
        IAdminDashboardService dashboardService)
    {
        _unitOfWork = unitOfWork;
        _adminService = adminService;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? range = "month", CancellationToken cancellationToken = default)
    {
        var model = await _dashboardService.GetDashboardAsync(range, cancellationToken);
        ViewData["PendingChefApplicationsCount"] = model.PendingApplicationsCount;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Chefs(string? status = "Pending")
    {
        string normalizedStatus = ValidApplicationStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        var allApplications = await _adminService.GetChefApplicationsAsync("all");
        ViewData["PendingChefApplicationsCount"] = allApplications.Count(c => c.Status == "Pending");
        ViewBag.CurrentStatus = normalizedStatus;

        var filtered = normalizedStatus == "all"
            ? allApplications
            : allApplications.Where(c => c.Status == normalizedStatus).ToList();

        return View(filtered);
    }

    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settingsList = await _unitOfWork.PlatformSettingRepository.GetAllAsync();
        var settings = settingsList.FirstOrDefault() ?? new PlatformSettings();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(decimal commissionRate)
    {
        if (commissionRate < 5 || commissionRate > 35)
        {
            TempData["ErrorMessage"] = "Commission rate must be between 5% and 35%.";
            return RedirectToAction(nameof(Settings));
        }

        decimal decimalRate = Math.Round(commissionRate / 100m, 4);
        var settingsList = await _unitOfWork.PlatformSettingRepository.GetAllAsync();
        var settings = settingsList.FirstOrDefault();

        if (settings is null)
        {
            settings = new PlatformSettings
            {
                CommissionRate = decimalRate,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PlatformSettingRepository.AddAsync(settings);
        }
        else
        {
            settings.CommissionRate = decimalRate;
            settings.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PlatformSettingRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Platform commission rate updated to {commissionRate:0.##}% successfully.";
        return RedirectToAction(nameof(Settings));
    }

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
    public async Task<IActionResult> ApproveApplication(int id, string? approvalNotes)
    {
        var success = await _adminService.ApproveApplicationAsync(id, approvalNotes);
        if (!success)
        {
            TempData["ErrorMessage"] = "This application was not found or has already been processed.";
            return RedirectToAction(nameof(Chefs));
        }

        TempData["SuccessMessage"] = "Chef application has been approved successfully.";
        return RedirectToAction(nameof(Chefs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int id, string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            TempData["ErrorMessage"] = "A rejection reason is required.";
            return RedirectToAction(nameof(ReviewApplication), new { id });
        }

        var success = await _adminService.RejectApplicationAsync(id, rejectionReason);
        if (!success)
        {
            TempData["ErrorMessage"] = "This application was not found or has already been processed.";
            return RedirectToAction(nameof(Chefs));
        }

        TempData["SuccessMessage"] = "Chef application has been rejected.";
        return RedirectToAction(nameof(Chefs));
    }
}
