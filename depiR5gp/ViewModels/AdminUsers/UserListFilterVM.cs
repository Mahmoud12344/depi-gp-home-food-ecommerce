namespace depiR5gp.ViewModels.AdminUsers;

/// <summary>Query-string filters for the admin account list (all optional).</summary>
public class UserListFilterVM
{
    /// <summary>Free-text search over name, email and username.</summary>
    public string? Query { get; set; }

    /// <summary>"all" | "customer" | "chef".</summary>
    public string Role { get; set; } = "all";

    /// <summary>"all" | "active" | "suspended" | "deactivated".</summary>
    public string Status { get; set; } = "all";

    public int Page { get; set; } = 1;
}
