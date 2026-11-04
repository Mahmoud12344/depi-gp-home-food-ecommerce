using depiR5gp.ViewModels.AdminUsers;

namespace depiR5gp.Services.Interfaces;

public interface IUserManagementService
{
    // Paginated, filtered list of customer and chef accounts
    Task<UserListVM> GetUsersAsync(UserListFilterVM filter);

    // Suspend / deactivate / reactivate an account and revoke its active sessions
    Task<(bool IsSuccess, string Message)> ChangeStatusAsync(int adminId, ChangeUserStatusVM model);
}
