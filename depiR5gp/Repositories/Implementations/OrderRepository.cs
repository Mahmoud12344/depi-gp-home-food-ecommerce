using depiR5gp.Data;
using depiR5gp.Models.Orders;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class OrderRepository(ApplicationDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
}

