namespace depiR5gp.ViewModels.AdminUsers;

/// <summary>Page model for /Admin/Users.</summary>
public class UserListVM
{
    public IReadOnlyList<UserListItemVM> Items { get; set; } = [];
    public UserListFilterVM Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public int FirstItemNumber => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int LastItemNumber => Math.Min(Page * PageSize, TotalCount);
}
