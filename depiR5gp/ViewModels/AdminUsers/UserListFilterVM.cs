using Microsoft.AspNetCore.Mvc;

namespace depiR5gp.ViewModels.AdminUsers;

/// <summary>Query-string filters for the admin account list (all optional).</summary>
public class UserListFilterVM
{
    private string? _query;

    /// <summary>Free-text search over name, email, username and kitchen name.</summary>
    [FromQuery(Name = "q")]
    public string? Query
    {
        get => _query;
        set => _query = value;
    }

    /// <summary>Alias to support ?query= alongside ?q=.</summary>
    [FromQuery(Name = "query")]
    public string? QueryAlias
    {
        get => _query;
        set
        {
            if (string.IsNullOrWhiteSpace(_query) && !string.IsNullOrWhiteSpace(value))
            {
                _query = value;
            }
        }
    }

    /// <summary>"all" | "customer" | "chef".</summary>
    [FromQuery(Name = "role")]
    public string Role { get; set; } = "all";

    /// <summary>"all" | "active" | "suspended" | "deactivated".</summary>
    [FromQuery(Name = "status")]
    public string Status { get; set; } = "all";

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;
}
