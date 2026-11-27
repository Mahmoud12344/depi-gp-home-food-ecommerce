using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Platform;
using depiR5gp.Models.Support;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using System.Threading.Tasks;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private static readonly string[] ValidReviewStatuses = ["Pending", "Approved", "Rejected"];
    private static readonly string[] ValidApplicationStatuses = ["Pending", "Approved", "Rejected", "Suspended"];
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _dashboardService;
    private readonly ApplicationDbContext _context;

    public AdminController(
        IUnitOfWork unitOfWork,
        IAdminService adminService,
        IAdminDashboardService dashboardService,
        ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _adminService = adminService;
        _dashboardService = dashboardService;
        _context = context;
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
    public async Task<IActionResult> Settings(string? faqSearch = null)
    {
        var settingsList = await _unitOfWork.PlatformSettingRepository.GetAllAsync();
        var settings = settingsList.FirstOrDefault() ?? new PlatformSettings();

        IQueryable<Faq> faqQuery = _context.Faqs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(faqSearch))
        {
            string term = faqSearch.Trim();
            faqQuery = faqQuery.Where(f => f.Question.Contains(term) || f.Answer.Contains(term));
        }

        ViewBag.Faqs = await faqQuery.OrderByDescending(f => f.UpdatedAt).ThenByDescending(f => f.Id).ToListAsync();
        ViewBag.FaqSearch = faqSearch;
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(
        string? actionType,
        decimal? commissionRate,
        string? question,
        string? category,
        string? answer,
        int? faqId)
    {
        int? adminUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : null;

        if (string.Equals(actionType, "AddFaq", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                TempData["ErrorMessage"] = "Both question and verified answer are required to add an FAQ entry.";
                return RedirectToAction(nameof(Settings));
            }

            string cleanCategory = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
            string storedQuestion = $"[{cleanCategory}] {question.Trim()}";

            _context.Faqs.Add(new Faq
            {
                Question = storedQuestion.Length > 300 ? storedQuestion[..300] : storedQuestion,
                Answer = answer.Trim(),
                UpdatedByUserId = adminUserId,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "FAQ entry added to AI knowledge base.";
            return RedirectToAction(nameof(Settings));
        }

        if (string.Equals(actionType, "DeleteFaq", StringComparison.OrdinalIgnoreCase))
        {
            if (faqId.HasValue)
            {
                var faq = await _context.Faqs.FindAsync(faqId.Value);
                if (faq is not null)
                {
                    _context.Faqs.Remove(faq);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "FAQ entry removed.";
                    return RedirectToAction(nameof(Settings));
                }
            }

            TempData["ErrorMessage"] = "FAQ entry not found.";
            return RedirectToAction(nameof(Settings));
        }

        if (commissionRate is null or < 5 or > 35)
        {
            TempData["ErrorMessage"] = "Commission rate must be between 5% and 35%.";
            return RedirectToAction(nameof(Settings));
        }

        decimal decimalRate = Math.Round(commissionRate.Value / 100m, 4);
        var settingsList = await _unitOfWork.PlatformSettingRepository.GetAllAsync();
        var settings = settingsList.FirstOrDefault();

        if (settings is null)
        {
            settings = new PlatformSettings
            {
                CommissionRate = decimalRate,
                UpdatedByUserId = adminUserId,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PlatformSettingRepository.AddAsync(settings);
        }
        else
        {
            settings.CommissionRate = decimalRate;
            settings.UpdatedByUserId = adminUserId;
            settings.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PlatformSettingRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Platform commission rate updated to {commissionRate.Value:0.##}% successfully.";
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
