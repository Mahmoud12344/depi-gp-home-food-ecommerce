using depiR5gp.Models.Users;
using depiR5gp.ViewModels.AdminUsers;

namespace depiR5gp.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<AppUser>
{
    // One page of customer/chef accounts matching the filter, plus the total match count
    Task<(IReadOnlyList<UserListItemVM> Items, int Total)> GetPagedAsync(UserListFilterVM filter, int pageSize);

    Task<bool> IsInAnyRoleAsync(int userId, params string[] roles);

    Task AddStatusHistoryAsync(UserStatusHistory entry);
}
