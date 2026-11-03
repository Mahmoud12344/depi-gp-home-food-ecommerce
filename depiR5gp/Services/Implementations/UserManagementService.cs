using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels.AdminUsers;

namespace depiR5gp.Services.Implementations;

public class UserManagementService(IUnitOfWork unitOfWork) : IUserManagementService
{
    private const int PageSize = 10;
    private const int MaxReasonLength = 200; // UserStatusHistories.reason column size

    private static readonly string[] ManagedRoles = [AppRoles.Customer, AppRoles.Chef];

    public async Task<UserListVM> GetUsersAsync(UserListFilterVM filter)
    {
        filter.Query = filter.Query?.Trim();
        filter.Role = (filter.Role ?? "all").ToLowerInvariant();
        filter.Status = (filter.Status ?? "all").ToLowerInvariant();

        var (items, total) = await unitOfWork.Users.GetPagedAsync(filter, PageSize);

        var model = new UserListVM
        {
            Items = items,
            Filter = filter,
            TotalCount = total,
            PageSize = PageSize
        };
        model.Page = Math.Clamp(filter.Page, 1, model.TotalPages);

        return model;
    }

    public async Task<(bool IsSuccess, string Message)> ChangeStatusAsync(int adminId, ChangeUserStatusVM model)
    {
        if (!Enum.IsDefined(model.NewStatus))
        {
            return (false, "Invalid account status.");
        }

        if (model.UserId == adminId)
        {
            return (false, "You cannot change the status of your own account.");
        }

        var user = await unitOfWork.Users.GetByIdAsync(model.UserId);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (!await unitOfWork.Users.IsInAnyRoleAsync(user.Id, ManagedRoles))
        {
            return (false, "Only customer and chef accounts can be managed here.");
        }

        if (user.Status == model.NewStatus)
        {
            return (false, $"This account is already {Describe(model.NewStatus)}.");
        }

        string? reason = model.BuildReason();

        if (model.NewStatus != UserStatus.Active && string.IsNullOrWhiteSpace(reason))
        {
            return (false, "A reason is required to suspend or deactivate an account.");
        }

        if (reason is { Length: > MaxReasonLength })
        {
            return (false, $"The reason must be {MaxReasonLength} characters or fewer.");
        }

        user.Status = model.NewStatus;

        // Rotating the security stamp invalidates every auth cookie issued to this user;
        // the cookie validator (interval = 0, see Program.cs) rejects them on the next request.
        user.SecurityStamp = Guid.NewGuid().ToString();

        await unitOfWork.Users.AddStatusHistoryAsync(new UserStatusHistory
        {
            UserId = user.Id,
            ChangedByUserId = adminId,
            Status = model.NewStatus,
            Reason = reason,
            ChangedAt = DateTime.UtcNow
        });

        // Status, security stamp and audit row are saved in a single transaction
        await unitOfWork.SaveChangesAsync();

        string name = $"{user.FirstName} {user.LastName}".Trim();
        return (true, model.NewStatus switch
        {
            UserStatus.Active => $"{name}'s account has been reactivated.",
            UserStatus.Suspended => $"{name}'s account has been suspended and their sessions revoked.",
            _ => $"{name}'s account has been deactivated and their sessions revoked."
        });
    }

    private static string Describe(UserStatus status) => status switch
    {
        UserStatus.Active => "active",
        UserStatus.Suspended => "suspended",
        _ => "deactivated"
    };
}
