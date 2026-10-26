using depiR5gp.Data;
using depiR5gp.Models.Cart;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class CartRepository(ApplicationDbContext context) : GenericRepository<Models.Cart.Cart>(context), ICartRepository
{
}

