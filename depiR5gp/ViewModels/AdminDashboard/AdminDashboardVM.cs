namespace depiR5gp.ViewModels.AdminDashboard;

public class AdminDashboardVM
{
    public string DateRangeKey { get; set; } = "month";
    public string DateRangeLabel { get; set; } = "This Month";

    // Row 1: Revenue & Order Summary Cards
    public decimal GrossRevenue { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal AllTimeRevenue { get; set; }
    public int AllTimeOrdersCount { get; set; }

    public int TotalOrders { get; set; }
    public int DeliveredOrdersCount { get; set; }
    public int PendingOrdersCount { get; set; }

    // Row 2: Platform Health KPI Cards
    public int TotalUsers { get; set; }
    public int ActiveKitchensCount { get; set; }
    public int PendingApplicationsCount { get; set; }
    public int TotalDishesCount { get; set; }

    // Row 5: Financial Breakdown
    public decimal CardPaymentsTotal { get; set; }
    public decimal CashCollectionsTotal { get; set; }
    public decimal ChefPayoutLiabilities { get; set; }

    // Row 3 & 4: Chart.js Datasets
    public List<string> RevenueLabels { get; set; } = [];
    public List<decimal> RevenueValues { get; set; } = [];

    public List<string> TopKitchenNames { get; set; } = [];
    public List<decimal> TopKitchenRevenues { get; set; } = [];
    public List<int> TopKitchenOrderCounts { get; set; } = [];
}
