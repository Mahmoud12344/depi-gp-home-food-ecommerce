using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Orders;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class OrderRepository(ApplicationDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<List<Order>> GetValidOrdersWithChefAsync(DateTime? startDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(o => o.Status != "Cancelled" && o.Status != "Rejected")
            .Include(o => o.ChefProfile)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(o => o.Status != "Cancelled" && o.Status != "Rejected");

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        return await query.SumAsync(o => (decimal?)o.Total, cancellationToken) ?? 0m;
    }

    public async Task<int> GetValidOrdersCountAsync(DateTime? startDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(o => o.Status != "Cancelled" && o.Status != "Rejected");

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}


