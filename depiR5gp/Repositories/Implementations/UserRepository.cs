using depiR5gp.Data;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.ViewModels.AdminUsers;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Repositories.Implementations;

public class UserRepository(ApplicationDbContext context) : GenericRepository<AppUser>(context), IUserRepository
{
    public async Task<(IReadOnlyList<UserListItemVM> Items, int Total)> GetPagedAsync(UserListFilterVM filter, int pageSize)
    {
        string[] roleNames = filter.Role switch
        {
            "customer" => [AppRoles.Customer],
            "chef" => [AppRoles.Chef],
            _ => [AppRoles.Customer, AppRoles.Chef]
        };

        UserStatus? status = filter.Status switch
        {
            "active" => UserStatus.Active,
            "suspended" => UserStatus.Suspended,
            "deactivated" => UserStatus.Deleted,
            _ => null
        };

        // Only customer/chef accounts are ever listed (admins and support agents are excluded)
        var query = _context.Users.AsNoTracking()
            .Where(u => _context.UserRoles.Any(ur =>
                ur.UserId == u.Id &&
                _context.Roles.Any(r => r.Id == ur.RoleId && roleNames.Contains(r.Name!))));

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        string? term = filter.Query?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(u =>
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term) ||
                (u.FirstName + " " + u.LastName).Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.UserName != null && u.UserName.Contains(term)));
        }

        int total = await query.CountAsync();

        int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        int page = Math.Clamp(filter.Page, 1, totalPages);

        var items = await query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemVM
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email ?? string.Empty,
                Role = _context.UserRoles.Any(ur =>
                           ur.UserId == u.Id &&
                           _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == AppRoles.Chef))
                    ? AppRoles.Chef
                    : AppRoles.Customer,
                Status = u.Status,
                OrdersCount = u.Orders.Count() + (u.ChefProfile != null ? u.ChefProfile.Orders.Count() : 0),
                LastReason = _context.UserStatusHistories
                    .Where(h => h.UserId == u.Id)
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => h.Reason)
                    .FirstOrDefault(),
                LastChangedAt = _context.UserStatusHistories
                    .Where(h => h.UserId == u.Id)
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => (DateTime?)h.ChangedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> IsInAnyRoleAsync(int userId, params string[] roles)
    {
        return await _context.UserRoles.AnyAsync(ur =>
            ur.UserId == userId &&
            _context.Roles.Any(r => r.Id == ur.RoleId && roles.Contains(r.Name!)));
    }

    public async Task AddStatusHistoryAsync(UserStatusHistory entry)
    {
        await _context.UserStatusHistories.AddAsync(entry);
    }
}
