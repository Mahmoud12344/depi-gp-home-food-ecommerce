using depiR5gp.Models.Users;

namespace depiR5gp.ViewModels.AdminUsers;

/// <summary>One row of the admin account list.</summary>
public class UserListItemVM
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>"Customer" or "Chef".</summary>
    public string Role { get; set; } = string.Empty;

    public UserStatus Status { get; set; }
    public int OrdersCount { get; set; }

    /// <summary>Reason recorded with the most recent status change (if any).</summary>
    public string? LastReason { get; set; }

    public DateTime? LastChangedAt { get; set; }
}
