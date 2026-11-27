using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Payments;
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
    private static readonly string[] ActiveOrderStatuses = ["Pending", "Accepted", "Preparing", "Ready"];
    private static readonly string[] HistoricalOrderStatuses = ["Completed", "Cancelled", "Rejected"];

    public async Task<IActionResult> Index()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

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

    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        await PopulateHeaderAsync(chef);

        var liveOrders = await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Options)
            .Where(o => o.ChefProfileId == chef.Id && ActiveOrderStatuses.Contains(o.Status))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        var vm = new ChefLiveOrdersVM
        {
            Orders = liveOrders.Select(o => new ChefLiveOrderCardVM
            {
                OrderId = o.Id,
                Reference = o.Reference,
                Status = o.Status == "Accepted" ? "Preparing" : o.Status,
                CustomerName = o.User is not null
                    ? $"{o.User.FirstName} {o.User.LastName}".Trim()
                    : $"Customer #{o.UserId}",
                PaymentMethod = o.PaymentMethod,
                Total = o.Total,
                CreatedAt = o.CreatedAt,
                Items = o.Items.OrderBy(i => i.LineNo).Select(i => new ChefLiveOrderItemVM
                {
                    DishName = i.DishName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Note = i.Note,
                    OptionLabels = i.Options.Select(opt =>
                        opt.ExtraPrice > 0
                            ? $"{opt.OptionName} (+{opt.ExtraPrice:0.##} EGP)"
                            : opt.OptionName).ToList()
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string nextStatus)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        if (nextStatus is not ("Preparing" or "Ready" or "Completed"))
        {
            TempData["ErrorMessage"] = "Invalid order status transition.";
            return RedirectToAction(nameof(Orders));
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ChefProfileId == chef.Id);
        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Orders));
        }

        bool validTransition = (order.Status, nextStatus) switch
        {
            ("Pending", "Preparing") => true,
            ("Accepted", "Preparing") => true,
            ("Accepted", "Ready") => true,
            ("Preparing", "Ready") => true,
            ("Ready", "Completed") => true,
            _ => false
        };

        if (!validTransition)
        {
            TempData["ErrorMessage"] = $"Cannot transition order #{order.Reference} from {order.Status} to {nextStatus}.";
            return RedirectToAction(nameof(Orders));
        }

        order.Status = nextStatus;
        if (nextStatus == "Completed")
        {
            order.PaymentStatus = "Paid";
        }

        int? chefUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : null;
        context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            ChangedByUserId = chefUserId,
            Status = nextStatus,
            ChangedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Order #{order.Reference} updated to {nextStatus}.";
        return RedirectToAction(nameof(Orders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOrder(int orderId, string? rejectReason, string? rejectionNote)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ChefProfileId == chef.Id);
        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Orders));
        }

        if (order.Status != "Pending")
        {
            TempData["ErrorMessage"] = "Only pending orders can be rejected.";
            return RedirectToAction(nameof(Orders));
        }

        string reasonLabel = rejectReason switch
        {
            "capacity" => "Kitchen at maximum capacity for this time slot",
            "ingredients" => "Fresh ingredients out of stock for today",
            "early_close" => "Kitchen counter closed early today",
            "instructions" => "Special customer instructions cannot be accommodated",
            _ => "Order declined by kitchen"
        };

        string? note = rejectionNote?.Trim();
        string fullReason = string.IsNullOrWhiteSpace(note) ? reasonLabel : $"{reasonLabel} — {note}";
        if (fullReason.Length > 500)
        {
            fullReason = fullReason[..500];
        }

        order.Status = "Rejected";
        order.CancellationReason = fullReason;

        int? chefUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int uid) ? uid : null;
        context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            ChangedByUserId = chefUserId,
            Status = "Rejected",
            ChangedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Order #{order.Reference} has been rejected.";
        return RedirectToAction(nameof(Orders));
    }

    [HttpGet]
    public async Task<IActionResult> History(string? status = "all")
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        await PopulateHeaderAsync(chef);

        string normalizedStatus = HistoricalOrderStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.ChefProfileId == chef.Id && HistoricalOrderStatuses.Contains(o.Status));

        if (normalizedStatus != "all")
        {
            query = query.Where(o => o.Status == normalizedStatus);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToListAsync();

        var vm = new ChefOrderHistoryVM
        {
            SelectedStatus = normalizedStatus,
            Orders = orders.Select(o =>
            {
                var itemLabels = o.Items.OrderBy(i => i.LineNo)
                    .Select(i => i.Quantity > 1 ? $"{i.DishName} (x{i.Quantity})" : i.DishName)
                    .ToList();

                return new ChefHistoryOrderRowVM
                {
                    OrderId = o.Id,
                    Reference = o.Reference,
                    Status = o.Status,
                    DishSummary = itemLabels.Count > 0 ? string.Join(", ", itemLabels) : $"Order #{o.Reference}",
                    CustomerName = o.User is not null
                        ? $"{o.User.FirstName} {o.User.LastName}".Trim()
                        : $"Customer #{o.UserId}",
                    CreatedAt = o.CreatedAt,
                    GrossTotal = o.Total,
                    NetEarnings = o.Status == "Completed" ? Math.Max(0m, o.Total - o.CommissionAmount) : 0m,
                    CancellationReason = o.CancellationReason
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Reviews()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        await PopulateHeaderAsync(chef);

        var reviews = await context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Order)
                .ThenInclude(o => o.Items)
            .Where(r => r.ChefProfileId == chef.Id && r.ModerationStatus == "Approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var vm = new ChefReviewsTabVM
        {
            TotalReviewsCount = reviews.Count,
            AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => (double)r.Rating), 1) : 0.0,
            Reviews = reviews.Select(r =>
            {
                var dishes = r.Order?.Items?
                    .OrderBy(i => i.LineNo)
                    .Select(i => i.DishName)
                    .ToList() ?? [];

                return new ChefReviewItemVM
                {
                    ReviewId = r.Id,
                    CustomerName = r.User is not null
                        ? $"{r.User.FirstName} {r.User.LastName}".Trim()
                        : $"Customer #{r.UserId}",
                    DishSummary = dishes.Count > 0 ? string.Join(", ", dishes) : $"Order #{r.Order?.Reference ?? r.OrderId.ToString()}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Wallet()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        await PopulateHeaderAsync(chef);

        var completedOrders = await context.Orders
            .AsNoTracking()
            .Where(o => o.ChefProfileId == chef.Id && o.Status == "Completed")
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .ToListAsync();

        var payouts = await context.Payouts
            .AsNoTracking()
            .Where(p => p.ChefProfileId == chef.Id && p.Status != "Rejected")
            .OrderBy(p => p.Id)
            .ToListAsync();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        decimal currentMonthCommission = completedOrders
            .Where(o => o.CreatedAt >= monthStart)
            .Sum(o => o.CommissionAmount);

        decimal totalNetEarned = completedOrders.Sum(o => Math.Max(0m, o.Total - o.CommissionAmount));
        decimal totalPayouts = payouts.Sum(p => p.Amount);
        decimal availableBalance = Math.Max(0m, totalNetEarned - totalPayouts);

        decimal rate = chef.CommissionRate ?? 0.15m;
        decimal ratePct = rate <= 1m ? Math.Round(rate * 100m, 1) : Math.Round(rate, 1);

        var rawEvents = new List<(DateTime OccurredAt, string Reference, string TypeLabel, decimal? Gross, decimal? Commission, decimal NetDelta)>();

        foreach (var o in completedOrders)
        {
            decimal net = Math.Max(0m, o.Total - o.CommissionAmount);
            rawEvents.Add((
                o.CreatedAt,
                o.Reference,
                "Completed Order Credit",
                o.Total,
                o.CommissionAmount,
                net));
        }

        foreach (var p in payouts)
        {
            rawEvents.Add((
                DateTime.UtcNow,
                p.Reference ?? $"PAYOUT-{p.Id:D4}",
                $"Payout ({p.Status})",
                null,
                null,
                -p.Amount));
        }

        var orderedEvents = rawEvents.OrderBy(e => e.OccurredAt).ToList();
        decimal running = 0m;
        var ledgerRows = new List<ChefWalletLedgerRowVM>(orderedEvents.Count);

        foreach (var ev in orderedEvents)
        {
            running += ev.NetDelta;
            ledgerRows.Add(new ChefWalletLedgerRowVM
            {
                OccurredAt = ev.OccurredAt,
                Reference = ev.Reference,
                TypeLabel = ev.TypeLabel,
                GrossAmount = ev.Gross,
                CommissionAmount = ev.Commission,
                NetDelta = ev.NetDelta,
                RunningBalance = Math.Max(0m, running)
            });
        }

        ledgerRows.Reverse();

        return View(new ChefWalletVM
        {
            AvailableBalance = availableBalance,
            CurrentMonthCommission = currentMonthCommission,
            EffectiveCommissionPercent = ratePct,
            TotalDisbursed = totalPayouts,
            LedgerRows = ledgerRows
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPayout()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        decimal totalNetEarned = await context.Orders
            .AsNoTracking()
            .Where(o => o.ChefProfileId == chef.Id && o.Status == "Completed")
            .SumAsync(o => (decimal?)(o.Total - o.CommissionAmount)) ?? 0m;

        decimal totalPayouts = await context.Payouts
            .AsNoTracking()
            .Where(p => p.ChefProfileId == chef.Id && p.Status != "Rejected")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        decimal availableBalance = Math.Round(totalNetEarned - totalPayouts, 2);
        if (availableBalance <= 0m)
        {
            TempData["ErrorMessage"] = "You have no available balance to withdraw at this time.";
            return RedirectToAction(nameof(Wallet));
        }

        var payout = new Payout
        {
            ChefProfileId = chef.Id,
            Amount = availableBalance,
            Status = "Pending",
            Reference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            IsSimulated = true
        };

        context.Payouts.Add(payout);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Payout request for {availableBalance:N2} EGP ({payout.Reference}) has been submitted.";
        return RedirectToAction(nameof(Wallet));
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

        if (chef.Status != "Approved")
        {
            return IsAjaxRequest()
                ? BadRequest(new { success = false, message = "Your kitchen must be approved before accepting orders." })
                : RedirectToAction("Index", "Dashboard");
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
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

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
