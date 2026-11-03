using System.ComponentModel.DataAnnotations;
using depiR5gp.Models.Users;

namespace depiR5gp.ViewModels.AdminUsers;

/// <summary>Posted by the suspend / deactivate / reactivate forms.</summary>
public class ChangeUserStatusVM
{
    [Required]
    public int UserId { get; set; }

    [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid account status.")]
    public UserStatus NewStatus { get; set; }

    [StringLength(100, ErrorMessage = "Reason must be 100 characters or fewer.")]
    public string? Reason { get; set; }

    [StringLength(90, ErrorMessage = "Note must be 90 characters or fewer.")]
    public string? Note { get; set; }

    public string? ReturnUrl { get; set; }

    /// <summary>Combines the chosen reason and the optional note (max 193 chars, column limit is 200).</summary>
    public string? BuildReason()
    {
        string reason = Reason?.Trim() ?? string.Empty;
        string note = Note?.Trim() ?? string.Empty;

        if (reason.Length == 0 && note.Length == 0)
        {
            return null;
        }

        return note.Length == 0 ? reason : reason.Length == 0 ? note : $"{reason}: {note}";
    }
}
