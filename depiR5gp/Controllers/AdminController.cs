using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Payments;
using depiR5gp.Models.Platform;
using depiR5gp.Models.Support;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Implementations;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels.AdminDashboard;
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
    private readonly IChefService _chefService;

    public AdminController(
        IUnitOfWork unitOfWork,
        IAdminService adminService,
        IAdminDashboardService dashboardService,
        IChefService chefService)
    {
        _unitOfWork = unitOfWork;
        _adminService = adminService;
        _dashboardService = dashboardService;
        _chefService = chefService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? range = "month", CancellationToken cancellationToken = default)
    {
        var model = await _dashboardService.GetDashboardAsync(range, cancellationToken);
        ViewData["PendingChefApplicationsCount"] = model.PendingApplicationsCount;
        ViewBag.CommissionRate = await _chefService.GetCommissionRate();
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

    [HttpGet]
    public async Task<IActionResult> Orders(CancellationToken cancellationToken = default)
    {
        var dashboard = await _dashboardService.GetDashboardAsync("all", cancellationToken);
        ViewData["PendingChefApplicationsCount"] = dashboard.PendingApplicationsCount;

        var allOrders = await _unitOfWork.Orders.GetValidOrdersWithChefAsync(startDate: null, cancellationToken);

        decimal platformRate = dashboard.EffectiveCommissionRate;
        var rows = allOrders.Select(o =>
        {
            decimal rate = o.CommissionRate > 1m ? o.CommissionRate / 100m : (o.CommissionRate > 0 ? o.CommissionRate : platformRate);
            return new AdminOrderSettlementRowVM
            {
                OrderId = o.Id,
                Reference = o.Reference,
                ChefProfileId = o.ChefProfileId,
                KitchenName = o.ChefProfile?.KitchenName ?? $"Kitchen #{o.ChefProfileId}",
                CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : $"Customer #{o.UserId}",
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                GrossAmount = o.Total,
                CommissionRatePercent = Math.Round(rate * 100m, 2),
                CommissionAmount = o.CommissionAmount
            };
        }).ToList();

        return View(new AdminOrdersSettlementPageVM
        {
            EffectiveCommissionPercent = dashboard.EffectiveCommissionPercent,
            Orders = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> Payouts(CancellationToken cancellationToken = default)
    {
        var dashboard = await _dashboardService.GetDashboardAsync("all", cancellationToken);
        ViewData["PendingChefApplicationsCount"] = dashboard.PendingApplicationsCount;
        ViewBag.CommissionRate = dashboard.EffectiveCommissionRate;

        var allPayouts = await _unitOfWork.Orders.GetPayoutsWithDetailsAsync(cancellationToken);

        var payoutHistory = allPayouts.Select(p =>
        {
            var (channel, note) = AdminDashboardService.SplitPayoutReference(p.Reference, p.Id);
            var ts = p.LedgerEntries.OrderByDescending(l => l.CreatedAt).Select(l => (DateTime?)l.CreatedAt).FirstOrDefault()
                     ?? DateTime.UtcNow;
            return new AdminPayoutLogRowVM
            {
                PayoutId = p.Id,
                ChefProfileId = p.ChefProfileId,
                KitchenName = p.ChefProfile?.KitchenName ?? $"Kitchen #{p.ChefProfileId}",
                Amount = p.Amount,
                ChannelLabel = channel,
                ReferenceNote = note,
                Status = p.Status,
                CreatedAt = ts
            };
        }).ToList();

        return View(new AdminPayoutsPageVM
        {
            EffectiveCommissionPercent = dashboard.EffectiveCommissionPercent,
            CashRows = dashboard.CashReconciliationRows,
            ChefPayoutOptions = dashboard.ChefPayoutOptions,
            PayoutHistory = payoutHistory
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payouts(
        string? actionType,
        int chefId,
        decimal? amount,
        string? paymentMethod,
        string? referenceNote,
        int? payoutId,
        string? returnUrl,
        CancellationToken cancellationToken = default)
    {
        IActionResult RedirectBack() =>
            !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Payouts));

        var chef = await _unitOfWork.Chefs.GetByIdAsync(chefId);
        if (chef is null)
        {
            TempData["ErrorMessage"] = "Selected kitchen was not found.";
            return RedirectBack();
        }

        if (string.Equals(actionType, "ReconcileCash", StringComparison.OrdinalIgnoreCase))
        {
            decimal platformRate = await _chefService.GetCommissionRate();
            if (platformRate > 1m) platformRate /= 100m;
            decimal chefRate = chef.CommissionRate ?? platformRate;
            if (chefRate > 1m) chefRate /= 100m;

            var cashOrders = await _unitOfWork.Orders.FindAsync(
                o => o.ChefProfileId == chef.Id && o.PaymentMethod == "Cash" && o.Status == "Completed");

            decimal totalOwed = cashOrders.Sum(o => o.CommissionAmount > 0 ? o.CommissionAmount : Math.Round(o.Total * chefRate, 2));
            var reconciledEntries = await _unitOfWork.LedgerEntries.FindAsync(
                l => l.ChefProfileId == chef.Id && l.Type == "CashReconciled");
            decimal alreadyReconciled = reconciledEntries.Sum(l => l.Amount);

            decimal pendingOffset = Math.Max(0m, Math.Round(totalOwed - alreadyReconciled, 2));
            if (pendingOffset <= 0m)
            {
                TempData["InfoMessage"] = $"Cash commissions for {chef.KitchenName} are already reconciled.";
                return RedirectBack();
            }

            await _unitOfWork.LedgerEntries.AddAsync(new LedgerEntry
            {
                ChefProfileId = chef.Id,
                Type = "CashReconciled",
                Amount = pendingOffset,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reconciled {pendingOffset:N2} EGP in cash commissions for {chef.KitchenName}.";
            return RedirectBack();
        }

        decimal disburseAmount = Math.Round(amount ?? 0m, 2);
        if (disburseAmount <= 0m)
        {
            TempData["ErrorMessage"] = "Please enter a valid payout amount greater than zero.";
            return RedirectBack();
        }

        var completedChefOrders = await _unitOfWork.Orders.FindAsync(
            o => o.ChefProfileId == chef.Id && o.Status == "Completed");
        decimal completedNet = completedChefOrders.Sum(o => Math.Max(0m, o.Total - o.CommissionAmount));

        var completedChefPayouts = await _unitOfWork.Payouts.FindAsync(
            p => p.ChefProfileId == chef.Id && p.Status == "Completed");
        decimal completedPayouts = completedChefPayouts.Sum(p => p.Amount);

        decimal eligibleBalance = Math.Max(0m, Math.Round(completedNet - completedPayouts, 2));
        if (disburseAmount > eligibleBalance)
        {
            TempData["ErrorMessage"] = $"Payout amount ({disburseAmount:N2} EGP) exceeds {chef.KitchenName}'s eligible balance ({eligibleBalance:N2} EGP).";
            return RedirectBack();
        }

        string channelLabel = (paymentMethod ?? "instapay").ToLowerInvariant() switch
        {
            "bank" => "Bank Wire (IBAN)",
            "vodafone_cash" => "Vodafone Cash Wallet",
            _ => "InstaPay GPA"
        };

        string cleanRef = string.IsNullOrWhiteSpace(referenceNote)
            ? $"BATCH-{DateTime.UtcNow:yyyyMMdd}-{chef.Id:D2}"
            : referenceNote.Trim();
        string storedReference = $"{channelLabel} | {cleanRef}";
        if (storedReference.Length > 100) storedReference = storedReference[..100];

        Payout? payout = null;
        if (payoutId.HasValue)
        {
            var byId = await _unitOfWork.Payouts.GetByIdAsync(payoutId.Value);
            if (byId is not null && byId.ChefProfileId == chef.Id)
            {
                payout = byId;
            }
        }
        if (payout is null)
        {
            var pendingForChef = await _unitOfWork.Payouts.FindAsync(
                p => p.ChefProfileId == chef.Id && p.Status == "Pending");
            payout = pendingForChef.OrderBy(p => p.Id).FirstOrDefault();
        }

        if (payout is not null)
        {
            payout.Amount = disburseAmount;
            payout.Status = "Completed";
            payout.Reference = storedReference;
            _unitOfWork.Payouts.Update(payout);
        }
        else
        {
            payout = new Payout
            {
                ChefProfileId = chef.Id,
                Amount = disburseAmount,
                Status = "Completed",
                Reference = storedReference,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            };
            await _unitOfWork.Payouts.AddAsync(payout);
            await _unitOfWork.SaveChangesAsync();
        }

        await _unitOfWork.LedgerEntries.AddAsync(new LedgerEntry
        {
            ChefProfileId = chef.Id,
            PayoutId = payout.Id,
            Type = "Payout",
            Amount = -disburseAmount,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Disbursed {disburseAmount:N2} EGP to {chef.KitchenName} via {channelLabel}.";
        return RedirectBack();
    }

    [HttpGet]
    public async Task<IActionResult> Settings(string? faqSearch = null)
    {
        var settingsList = await _unitOfWork.PlatformSettingRepository.GetAllAsync();
        var settings = settingsList.FirstOrDefault() ?? new PlatformSettings();

        IEnumerable<Faq> faqs = string.IsNullOrWhiteSpace(faqSearch)
            ? await _unitOfWork.Faqs.GetAllAsync()
            : await _unitOfWork.Faqs.FindAsync(f =>
                f.Question.Contains(faqSearch.Trim()) || f.Answer.Contains(faqSearch.Trim()));

        ViewBag.Faqs = faqs.OrderByDescending(f => f.UpdatedAt).ThenByDescending(f => f.Id).ToList();
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

            await _unitOfWork.Faqs.AddAsync(new Faq
            {
                Question = storedQuestion.Length > 300 ? storedQuestion[..300] : storedQuestion,
                Answer = answer.Trim(),
                UpdatedByUserId = adminUserId,
                UpdatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "FAQ entry added to AI knowledge base.";
            return RedirectToAction(nameof(Settings));
        }

        if (string.Equals(actionType, "DeleteFaq", StringComparison.OrdinalIgnoreCase))
        {
            if (faqId.HasValue)
            {
                var faq = await _unitOfWork.Faqs.GetByIdAsync(faqId.Value);
                if (faq is not null)
                {
                    _unitOfWork.Faqs.Remove(faq);
                    await _unitOfWork.SaveChangesAsync();
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
