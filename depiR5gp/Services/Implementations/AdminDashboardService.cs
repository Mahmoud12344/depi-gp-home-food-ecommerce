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
        int deliveredCount = orders.Count(o => o.Status == "Delivered");
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

        return new AdminDashboardVM
        {
            DateRangeKey = key,
            DateRangeLabel = rangeLabel,
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
            CashCollectionsTotal = orders.Where(o => o.PaymentMethod == "Cash").Sum(o => o.Total),
            ChefPayoutLiabilities = Math.Max(0m, grossRevenue - commission),
            RevenueLabels = trend.Select(t => t.Label).ToList(),
            RevenueValues = trend.Select(t => t.Value).ToList(),
            TopKitchenNames = topKitchens.Select(k => k.Name).ToList(),
            TopKitchenRevenues = topKitchens.Select(k => k.Revenue).ToList(),
            TopKitchenOrderCounts = topKitchens.Select(k => k.OrderCount).ToList()
        };
    }
}

