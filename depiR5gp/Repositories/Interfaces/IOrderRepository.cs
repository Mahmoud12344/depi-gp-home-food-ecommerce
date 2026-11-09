using depiR5gp.Models.Orders;

namespace depiR5gp.Repositories.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<List<Order>> GetValidOrdersWithChefAsync(DateTime? startDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, CancellationToken cancellationToken = default);
    Task<int> GetValidOrdersCountAsync(DateTime? startDate = null, CancellationToken cancellationToken = default);
}


