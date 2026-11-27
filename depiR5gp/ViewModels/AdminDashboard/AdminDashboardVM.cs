namespace depiR5gp.ViewModels.AdminDashboard;

public class AdminOrderSettlementRowVM
{
    public int OrderId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentChannelLabel => string.Equals(PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase)
        ? "Cash on Pickup Counter"
        : "Stripe Card (Online Payment)";
    public string SettlementStatusLabel => string.Equals(Status, "Completed", StringComparison.OrdinalIgnoreCase)
        ? "Settled to Chef Wallet"
        : $"In Progress ({Status})";
    public decimal GrossAmount { get; set; }
    public decimal CommissionRatePercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetChefPayable => Math.Max(0m, GrossAmount - CommissionAmount);
}

public class AdminCashReconciliationRowVM
{
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public int CashOrdersCount { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CommissionRatePercent { get; set; }
    public decimal TotalCommissionOwed { get; set; }
    public decimal ReconciledCommission { get; set; }
    public decimal PendingCommissionOffset => Math.Max(0m, TotalCommissionOwed - ReconciledCommission);
    public bool IsSettled => PendingCommissionOffset <= 0.005m;
}

public class AdminChefPayoutOptionVM
{
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public decimal CompletedNetEarnings { get; set; }
    public decimal TotalDisbursed { get; set; }
    public decimal PendingRequestedAmount { get; set; }
    public decimal EligibleBalance => Math.Max(0m, CompletedNetEarnings - TotalDisbursed);
}

public class AdminPayoutLogRowVM
{
    public int PayoutId { get; set; }
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ChannelLabel { get; set; } = "InstaPay GPA";
    public string ReferenceNote { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public DateTime CreatedAt { get; set; }
}

public class AdminOrdersSettlementPageVM
{
    public decimal EffectiveCommissionPercent { get; set; } = 15m;
    public List<AdminOrderSettlementRowVM> Orders { get; set; } = [];
    public decimal TotalGrossGmv => Orders.Sum(o => o.GrossAmount);
    public decimal TotalCommission => Orders.Sum(o => o.CommissionAmount);
    public decimal TotalNetPayable => Orders.Sum(o => o.NetChefPayable);
}

public class AdminPayoutsPageVM
{
    public decimal EffectiveCommissionPercent { get; set; } = 15m;
    public decimal TotalCashCollected => CashRows.Sum(r => r.CashCollected);
    public decimal TotalCashCommission => CashRows.Sum(r => r.TotalCommissionOwed);
    public decimal PendingCashOffset => CashRows.Sum(r => r.PendingCommissionOffset);
    public List<AdminCashReconciliationRowVM> CashRows { get; set; } = [];
    public List<AdminChefPayoutOptionVM> ChefPayoutOptions { get; set; } = [];
    public List<AdminPayoutLogRowVM> PayoutHistory { get; set; } = [];
}

public class AdminDashboardVM
{
    public string DateRangeKey { get; set; } = "month";
    public string DateRangeLabel { get; set; } = "This Month";
    public decimal EffectiveCommissionRate { get; set; } = 0.15m;
    public decimal EffectiveCommissionPercent => Math.Round(EffectiveCommissionRate * 100m, 2);

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

    // Row 5: Financial Breakdown & Modal Data
    public decimal CardPaymentsTotal { get; set; }
    public decimal CashCollectionsTotal { get; set; }
    public decimal ChefPayoutLiabilities { get; set; }
    public decimal TotalCashCommission => CashReconciliationRows.Sum(r => r.TotalCommissionOwed);
    public decimal PendingCashOffsetTotal => CashReconciliationRows.Sum(r => r.PendingCommissionOffset);
    public List<AdminCashReconciliationRowVM> CashReconciliationRows { get; set; } = [];
    public List<AdminChefPayoutOptionVM> ChefPayoutOptions { get; set; } = [];
    public List<AdminPayoutLogRowVM> RecentPayouts { get; set; } = [];
    public List<AdminOrderSettlementRowVM> RecentSettlementOrders { get; set; } = [];

    // Row 3 & 4: Chart.js Datasets
    public List<string> RevenueLabels { get; set; } = [];
    public List<decimal> RevenueValues { get; set; } = [];

    public List<string> TopKitchenNames { get; set; } = [];
    public List<decimal> TopKitchenRevenues { get; set; } = [];
    public List<int> TopKitchenOrderCounts { get; set; } = [];
}
