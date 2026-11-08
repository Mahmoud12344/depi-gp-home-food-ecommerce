using depiR5gp.Data;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Repositories.Implementations;

public class ReviewRepository(ApplicationDbContext context) : GenericRepository<Review>(context), IReviewRepository
{
    public async Task<IReadOnlyList<Review>> GetForModerationAsync(string? status = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.ChefProfile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.ModerationStatus == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .ToListAsync();
    }
}


