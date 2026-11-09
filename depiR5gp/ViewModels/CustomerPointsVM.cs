namespace depiR5gp.ViewModels;

public class CustomerPointsVM
{
    public CustomerProfileVM Profile { get; set; } = new();
    public IReadOnlyList<CustomerPointsLedgerVM> Transactions { get; set; } = [];
}

public class CustomerPointsLedgerVM
{
    public string Type { get; set; } = string.Empty;
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OrderReference { get; set; }
}
