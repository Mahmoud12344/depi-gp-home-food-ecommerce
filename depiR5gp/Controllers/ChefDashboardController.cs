using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class ChefDashboardController(
    ApplicationDbContext context,
    IFileService fileService) : Controller
{
    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private static readonly string[] AllowedDocTypes = ["Identity", "HealthCertificate", "CulinaryLicense"];

    public async Task<IActionResult> Index()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        var header = await ChefPortalHeaderVM.LoadAsync(context, chef);
        ViewData["ChefHeader"] = header;

        var todayUtc = DateTime.UtcNow.Date;
        var windowStart = todayUtc.AddDays(-180);

        var recentOrders = await context.Orders
            .AsNoTracking()
            .Where(o => o.ChefProfileId == chef.Id && o.CreatedAt >= windowStart)
            .Select(o => new OrderAnalyticsRow(
                o.Id,
                o.Status,
                o.Total,
                o.CommissionAmount,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemAnalyticsRow(i.DishName, i.Quantity, i.UnitPrice)).ToList()))
            .ToListAsync();

        var approvedRatings = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ChefProfileId == chef.Id && r.ModerationStatus == "Approved")
            .Select(r => r.Rating)
            .ToListAsync();

        var vm = new ChefDashboardVM
        {
            Header = header,
            ReviewsCount = approvedRatings.Count,
            AverageRating = approvedRatings.Count > 0 ? Math.Round(approvedRatings.Average(r => (double)r), 1) : 0,
            Periods = new Dictionary<string, ChefDashboardPeriodVM>
            {
                ["7d"] = BuildPeriodAnalytics("7d", "7 days", 7, 1, todayUtc, recentOrders),
                ["30d"] = BuildPeriodAnalytics("30d", "30 days", 30, 3, todayUtc, recentOrders),
                ["90d"] = BuildPeriodAnalytics("90d", "90 days", 90, 10, todayUtc, recentOrders)
            }
        };

        return View(vm);
    }

    private sealed record OrderItemAnalyticsRow(string DishName, int Quantity, decimal UnitPrice);
    private sealed record OrderAnalyticsRow(
        int Id,
        string Status,
        decimal Total,
        decimal CommissionAmount,
        DateTime CreatedAt,
        List<OrderItemAnalyticsRow> Items);

    private static ChefDashboardPeriodVM BuildPeriodAnalytics(
        string key,
        string label,
        int totalDays,
        int bucketDays,
        DateTime todayUtc,
        List<OrderAnalyticsRow> allOrders)
    {
        var start = todayUtc.AddDays(-(totalDays - 1));
        var prevStart = start.AddDays(-totalDays);

        var curOrders = allOrders.Where(o => o.CreatedAt >= start).ToList();
        var prevOrders = allOrders.Where(o => o.CreatedAt >= prevStart && o.CreatedAt < start).ToList();

        var curCompleted = curOrders.Where(o => o.Status == "Completed").ToList();
        var prevCompleted = prevOrders.Where(o => o.Status == "Completed").ToList();

        var netEarnings = curCompleted.Sum(o => o.Total - o.CommissionAmount);
        var grossSales = curCompleted.Sum(o => o.Total);
        var prevNet = prevCompleted.Sum(o => o.Total - o.CommissionAmount);
        var avgOrder = curCompleted.Count > 0 ? Math.Round(grossSales / curCompleted.Count, 2) : 0m;

        // Chart 1: Timeline buckets
        var timelineLabels = new List<string>();
        var timelineNet = new List<decimal>();
        var timelineGross = new List<decimal>();
        var timelineOrders = new List<int>();

        int bucketCount = Math.Max(1, totalDays / bucketDays);
        for (int b = 0; b < bucketCount; b++)
        {
            var bStart = start.AddDays(b * bucketDays);
            var bEnd = b == bucketCount - 1 ? todayUtc.AddDays(1) : bStart.AddDays(bucketDays);

            var bucketOrders = curOrders.Where(o => o.CreatedAt >= bStart && o.CreatedAt < bEnd).ToList();
            var bucketCompleted = bucketOrders.Where(o => o.Status == "Completed").ToList();

            timelineLabels.Add(bucketDays == 1 ? bStart.ToString("ddd") : bStart.ToString("MMM d"));
            timelineNet.Add(Math.Round(bucketCompleted.Sum(o => o.Total - o.CommissionAmount), 2));
            timelineGross.Add(Math.Round(bucketCompleted.Sum(o => o.Total), 2));
            timelineOrders.Add(bucketOrders.Count);
        }

        // Chart 2: Orders by status
        int completedCnt = curOrders.Count(o => o.Status == "Completed");
        int activeCnt = curOrders.Count(o => o.Status is "Accepted" or "Preparing" or "Ready");
        int pendingCnt = curOrders.Count(o => o.Status == "Pending");
        int cancelledCnt = curOrders.Count(o => o.Status is "Cancelled" or "Rejected");
        int statusTotal = curOrders.Count;

        int Pct(int c) => statusTotal > 0 ? (int)Math.Round(c * 100.0 / statusTotal) : 0;

        var statusBreakdown = new List<StatusSliceVM>
        {
            new() { Label = "Completed", Tone = "teal", Count = completedCnt, Percent = Pct(completedCnt) },
            new() { Label = "In Progress", Tone = "ink", Count = activeCnt, Percent = Pct(activeCnt) },
            new() { Label = "Pending", Tone = "ochre", Count = pendingCnt, Percent = Pct(pendingCnt) },
            new() { Label = "Cancelled", Tone = "coral", Count = cancelledCnt, Percent = Pct(cancelledCnt) }
        };

        // Chart 3: Top 5 best-selling dishes
        var topDishes = curOrders
            .Where(o => o.Status is not ("Cancelled" or "Rejected"))
            .SelectMany(o => o.Items)
            .GroupBy(i => i.DishName)
            .Select(g => new TopDishStatVM
            {
                DishName = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = Math.Round(g.Sum(x => x.Quantity * x.UnitPrice), 2)
            })
            .OrderByDescending(x => x.Quantity)
            .ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        // Chart 4: Orders by day of week (Sun..Sat)
        string[] shortDays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        var weekdayOrders = Enumerable.Range(0, 7).Select(dow =>
        {
            var dayOrders = curOrders.Where(o => (int)o.CreatedAt.DayOfWeek == dow).ToList();
            int comp = dayOrders.Count(o => o.Status == "Completed");
            int other = dayOrders.Count - comp;
            decimal rev = Math.Round(dayOrders.Where(o => o.Status == "Completed").Sum(o => o.Total - o.CommissionAmount), 2);
            return new WeekdayOrderStatVM
            {
                Day = shortDays[dow],
                Completed = comp,
                Other = other,
                Revenue = rev
            };
        }).ToList();

        return new ChefDashboardPeriodVM
        {
            Key = key,
            Label = label,
            NetEarnings = Math.Round(netEarnings, 2),
            GrossSales = Math.Round(grossSales, 2),
            PrevNetEarnings = Math.Round(prevNet, 2),
            TotalOrders = curOrders.Count,
            PrevTotalOrders = prevOrders.Count,
            CompletedOrders = completedCnt,
            AverageOrderValue = avgOrder,
            TimelineLabels = timelineLabels,
            TimelineNet = timelineNet,
            TimelineGross = timelineGross,
            TimelineOrders = timelineOrders,
            StatusBreakdown = statusBreakdown,
            TopDishes = topDishes,
            WeekdayOrders = weekdayOrders
        };
    }

    public IActionResult Profile() => RedirectToAction("EditProfile", "Chefs");

    public async Task<IActionResult> Orders()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        await PopulateHeaderAsync(chef);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null)
        {
            return IsAjaxRequest()
                ? NotFound(new { success = false, message = "Chef profile not found." })
                : RedirectToAction("Onboarding", "Chefs");
        }

        chef.IsAcceptingOrders = !chef.IsAcceptingOrders;
        await context.SaveChangesAsync();

        var msg = chef.IsAcceptingOrders
            ? "Your kitchen is now open and taking orders."
            : "Your kitchen is now paused for new orders.";

        if (IsAjaxRequest())
        {
            return Json(new { success = true, isAcceptingOrders = chef.IsAcceptingOrders, message = msg });
        }

        TempData["SuccessMessage"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult WorkingHours() =>
        RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkingHours(ChefWorkingHoursVM model)
    {
        var chef = await GetCurrentChefAsync(includeWorkingHours: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        var errors = new List<string>();
        var newHours = new List<ChefWorkingHours>();
        foreach (var day in model.Days)
        {
            if (day.DayOfWeek > 6) continue;
            if (!day.IsOpen) continue;

            if (!TimeOnly.TryParse(day.OpenTime, out var openTime) || !TimeOnly.TryParse(day.CloseTime, out var closeTime))
            {
                errors.Add($"Invalid open/close time for {DayNames[day.DayOfWeek]}.");
                continue;
            }

            if (openTime >= closeTime)
            {
                errors.Add($"Close time must be after open time for {DayNames[day.DayOfWeek]}.");
                continue;
            }

            newHours.Add(new ChefWorkingHours
            {
                ChefProfileId = chef.Id,
                DayOfWeek = day.DayOfWeek,
                OpenTime = openTime,
                CloseTime = closeTime
            });
        }

        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(" ", errors);
            return RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });
        }

        context.ChefWorkingHours.RemoveRange(chef.WorkingHours);
        if (newHours.Count > 0)
        {
            await context.ChefWorkingHours.AddRangeAsync(newHours);
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Kitchen working hours updated successfully.";
        return RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });
    }

    [HttpGet]
    public IActionResult Documents() =>
        RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Documents(string docType, IFormFile? documentFile)
    {
        var chef = await GetCurrentChefAsync(includeDocuments: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        if (!AllowedDocTypes.Contains(docType))
        {
            TempData["ErrorMessage"] = "Invalid document type selected.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        if (documentFile is null || documentFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a document file to upload.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        if (documentFile.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Document file must be smaller than 5 MB.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        var ext = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
        if (!AllowedDocExtensions.Contains(ext))
        {
            TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        var url = await fileService.SaveAsync(documentFile, "chef_documents", docType);

        var existing = chef.Documents.FirstOrDefault(d =>
            string.Equals(d.DocType, docType, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            await fileService.DeleteAsync(existing.FileUrl);
            existing.FileUrl = url;
            existing.UploadedAt = DateTime.UtcNow;
        }
        else
        {
            context.ChefDocuments.Add(new ChefDocument
            {
                ChefProfileId = chef.Id,
                DocType = docType,
                FileUrl = url,
                UploadedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{docType} document uploaded and saved successfully.";
        return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
    }

    private async Task<ChefProfile?> GetCurrentChefAsync(bool includeWorkingHours = false, bool includeDocuments = false)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0) return null;

        IQueryable<ChefProfile> query = context.ChefProfiles;
        if (includeWorkingHours) query = query.Include(c => c.WorkingHours);
        if (includeDocuments) query = query.Include(c => c.Documents);

        return await query.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private async Task PopulateHeaderAsync(ChefProfile chef)
    {
        ViewData["ChefHeader"] = await ChefPortalHeaderVM.LoadAsync(context, chef);
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
