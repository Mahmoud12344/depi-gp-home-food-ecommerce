using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Payments;
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
            .Include(o => o.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Payout>> GetPayoutsWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payouts
            .AsNoTracking()
            .Include(p => p.ChefProfile)
            .Include(p => p.LedgerEntries)
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
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


