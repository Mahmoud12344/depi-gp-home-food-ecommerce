using System.Globalization;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels.AdminDashboard;

namespace depiR5gp.Services.Implementations;

public class AdminDashboardService(IUnitOfWork unitOfWork) : IAdminDashboardService
{
    public async Task<AdminDashboardVM> GetDashboardAsync(string? range = "month", CancellationToken cancellationToken = default)
    {
        string key = (range ?? "month").Trim().ToLowerInvariant();
        if (key is not ("7d" or "month" or "ytd" or "all"))
        {
            key = "month";
        }

        var today = DateTime.UtcNow.Date;
        DateTime? startDate = key switch
        {
            "7d"    => today.AddDays(-6),
            "month" => new DateTime(today.Year, today.Month, 1),
            "ytd"   => new DateTime(today.Year, 1, 1),
            _       => null
        };

        string rangeLabel = key switch
        {
            "7d"    => "Last 7 Days",
            "month" => "This Month",
            "ytd"   => "Year-to-Date",
            _       => "All Time"
        };

        var settingsList = await unitOfWork.PlatformSettingRepository.GetAllAsync();
        decimal platformCommissionRate = settingsList.FirstOrDefault()?.CommissionRate ?? 0.15m;
        if (platformCommissionRate > 1m) platformCommissionRate = Math.Round(platformCommissionRate / 100m, 4);

        // 1. Platform-wide counts
        int totalUsers = await unitOfWork.Users.CountAsync();
        int activeKitchens = await unitOfWork.Chefs.CountAsync(c => c.Status == "Approved");
        int pendingApps = await unitOfWork.Chefs.CountAsync(c => c.Status == "Pending");
        int totalDishes = await unitOfWork.Dishes.CountAsync(d => !d.IsArchived);

        int allTimeOrdersCount = await unitOfWork.Orders.GetValidOrdersCountAsync(startDate: null, cancellationToken);
        decimal allTimeRevenue = await unitOfWork.Orders.GetTotalRevenueAsync(startDate: null, cancellationToken);

        // 2. Period-filtered orders
        var orders = await unitOfWork.Orders.GetValidOrdersWithChefAsync(startDate, cancellationToken);

        decimal grossRevenue = orders.Sum(o => o.Total);
        decimal commission = orders.Sum(o => o.CommissionAmount);
        int deliveredCount = orders.Count(o => o.Status == "Completed");
        int pendingCount = orders.Count - deliveredCount;

        // 3. Top 5 Kitchens by Revenue (for Horizontal Bar Chart)
        var topKitchens = orders
            .GroupBy(o => o.ChefProfile?.KitchenName ?? $"Kitchen #{o.ChefProfileId}")
            .Select(g => new
            {
                Name = g.Key,
                Revenue = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        // 4. Revenue Trend Points (for Area Line Chart)
        var inv = CultureInfo.InvariantCulture;
        var trend = (key is "ytd" or "all") ? orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => (
                    Label: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", inv),
                    Value: g.Sum(o => o.Total)))
                .ToList()
            : orders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => (
                    Label: g.Key.ToString("MMM dd", inv),
                    Value: g.Sum(o => o.Total)))
                .ToList();

        // 5. Financial Modals Data via IUnitOfWork
        var allValidOrders = startDate is null
            ? orders
            : await unitOfWork.Orders.GetValidOrdersWithChefAsync(startDate: null, cancellationToken);

        var cashReconciledEntries = await unitOfWork.LedgerEntries.FindAsync(l => l.Type == "CashReconciled");
        var reconciledByChef = cashReconciledEntries
            .GroupBy(l => l.ChefProfileId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Amount));

        var cashRows = allValidOrders
            .Where(o => o.PaymentMethod == "Cash" && o.Status == "Completed")
            .GroupBy(o => new
            {
                o.ChefProfileId,
                KitchenName = o.ChefProfile != null ? o.ChefProfile.KitchenName : $"Kitchen #{o.ChefProfileId}",
                ChefRate = o.ChefProfile?.CommissionRate ?? platformCommissionRate
            })
            .Select(g =>
            {
                decimal rate = g.Key.ChefRate > 1m ? g.Key.ChefRate / 100m : g.Key.ChefRate;
                decimal owed = g.Sum(o => o.CommissionAmount > 0 ? o.CommissionAmount : Math.Round(o.Total * rate, 2));
                decimal reconciled = reconciledByChef.GetValueOrDefault(g.Key.ChefProfileId, 0m);
                return new AdminCashReconciliationRowVM
                {
                    ChefProfileId = g.Key.ChefProfileId,
                    KitchenName = g.Key.KitchenName,
                    CashOrdersCount = g.Count(),
                    CashCollected = g.Sum(o => o.Total),
                    CommissionRatePercent = Math.Round(rate * 100m, 2),
                    TotalCommissionOwed = owed,
                    ReconciledCommission = reconciled
                };
            })
            .OrderByDescending(r => r.PendingCommissionOffset)
            .ThenByDescending(r => r.CashCollected)
            .ToList();

        var approvedChefs = (await unitOfWork.Chefs.GetApplicationsAsync("Approved"))
            .OrderBy(c => c.KitchenName)
            .ToList();

        var payouts = await unitOfWork.Orders.GetPayoutsWithDetailsAsync(cancellationToken);

        var chefPayoutOptions = approvedChefs.Select(c =>
        {
            decimal netEarned = allValidOrders
                .Where(o => o.ChefProfileId == c.Id && o.Status == "Completed")
                .Sum(o => Math.Max(0m, o.Total - o.CommissionAmount));
            decimal disbursed = payouts
                .Where(p => p.ChefProfileId == c.Id && p.Status == "Completed")
                .Sum(p => p.Amount);
            decimal pendingReq = payouts
                .Where(p => p.ChefProfileId == c.Id && p.Status == "Pending")
                .Sum(p => p.Amount);

            return new AdminChefPayoutOptionVM
            {
                ChefProfileId = c.Id,
                KitchenName = c.KitchenName,
                CompletedNetEarnings = netEarned,
                TotalDisbursed = disbursed,
                PendingRequestedAmount = pendingReq
            };
        })
        .OrderByDescending(o => o.EligibleBalance)
        .ThenBy(o => o.KitchenName)
        .ToList();

        var recentPayouts = payouts
            .Take(10)
            .Select(p =>
            {
                var (channel, note) = SplitPayoutReference(p.Reference, p.Id);
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
            })
            .ToList();

        var settlementOrders = allValidOrders
            .Take(20)
            .Select(o =>
            {
                decimal rate = o.CommissionRate > 1m ? o.CommissionRate / 100m : (o.CommissionRate > 0 ? o.CommissionRate : platformCommissionRate);
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
            })
            .ToList();

        decimal totalEligibleLiabilities = chefPayoutOptions.Sum(c => c.EligibleBalance);

        return new AdminDashboardVM
        {
            DateRangeKey = key,
            DateRangeLabel = rangeLabel,
            EffectiveCommissionRate = platformCommissionRate,
            GrossRevenue = grossRevenue,
            PlatformCommission = commission,
            AllTimeRevenue = allTimeRevenue,
            AllTimeOrdersCount = allTimeOrdersCount,
            TotalOrders = orders.Count,
            DeliveredOrdersCount = deliveredCount,
            PendingOrdersCount = pendingCount,
            TotalUsers = totalUsers,
            ActiveKitchensCount = activeKitchens,
            PendingApplicationsCount = pendingApps,
            TotalDishesCount = totalDishes,
            CardPaymentsTotal = orders.Where(o => o.PaymentMethod != "Cash").Sum(o => o.Total),
            CashCollectionsTotal = orders.Where(o => o.PaymentMethod == "Cash" && o.Status == "Completed").Sum(o => o.Total),
            ChefPayoutLiabilities = totalEligibleLiabilities > 0 ? totalEligibleLiabilities : Math.Max(0m, grossRevenue - commission),
            CashReconciliationRows = cashRows,
            ChefPayoutOptions = chefPayoutOptions,
            RecentPayouts = recentPayouts,
            RecentSettlementOrders = settlementOrders,
            RevenueLabels = trend.Select(t => t.Label).ToList(),
            RevenueValues = trend.Select(t => t.Value).ToList(),
            TopKitchenNames = topKitchens.Select(k => k.Name).ToList(),
            TopKitchenRevenues = topKitchens.Select(k => k.Revenue).ToList(),
            TopKitchenOrderCounts = topKitchens.Select(k => k.OrderCount).ToList()
        };
    }

    public static (string Channel, string ReferenceNote) SplitPayoutReference(string? rawReference, int payoutId)
    {
        if (string.IsNullOrWhiteSpace(rawReference))
        {
            return ("InstaPay GPA", $"PAYOUT-{payoutId:D4}");
        }

        int sep = rawReference.IndexOf('|');
        if (sep > 0 && sep < rawReference.Length - 1)
        {
            return (rawReference[..sep].Trim(), rawReference[(sep + 1)..].Trim());
        }

        return ("InstaPay GPA", rawReference.Trim());
    }
}

