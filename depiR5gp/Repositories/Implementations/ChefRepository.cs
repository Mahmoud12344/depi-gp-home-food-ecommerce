using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Repositories.Implementations;

public class ChefRepository(ApplicationDbContext context) : GenericRepository<ChefProfile>(context), IChefRepository
{
    public async Task<bool> HasPendingOrApprovedApplicationAsync(int userId)
    {
        return await context.ChefProfiles
            .AnyAsync(c => c.UserId == userId && (c.Status == "Pending" || c.Status == "Approved"));
    }

    public async Task<List<ChefCardViewModel>> SearchAsync(ChefFilterViewModel f)
    {
        var q = _context.ChefProfiles.AsNoTracking().Where(c => c.Status == "Approved");

        if (f.Search is { } s)
            q = q.Where(c => c.KitchenName.Contains(s)
                          || (c.Bio != null && c.Bio.Contains(s))
                          || (c.User.FirstName + " " + c.User.LastName).Contains(s));
        if (f.Area != null) q = q.Where(c => c.Area == f.Area);

        return await q.OrderBy(c => c.KitchenName).Select(c => new ChefCardViewModel
        {
            Id = c.Id,
            KitchenName = c.KitchenName,
            ChefName = c.User.FirstName + " " + c.User.LastName,
            PhotoUrl = c.PhotoUrl,
            Bio = c.Bio,
            Area = c.Area,
            Rating = c.Reviews.Where(r => r.ModerationStatus == "Approved").Average(r => (double?)r.Rating) ?? 0,
            ReviewCount = c.Reviews.Count(r => r.ModerationStatus == "Approved")
        }).ToListAsync();
    }

    public async Task<List<FilterOption>> GetAreaOptionsAsync() =>
        await _context.ChefProfiles.AsNoTracking()
            .Where(c => c.Status == "Approved" && c.Area != null)
            .GroupBy(c => c.Area!)
            .Select(g => new FilterOption { Value = g.Key, Name = g.Key, Count = g.Count() })
            .ToListAsync();
}

