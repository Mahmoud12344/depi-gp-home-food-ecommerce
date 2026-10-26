using depiR5gp.Data;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class DishRepository(ApplicationDbContext context) : GenericRepository<Dish>(context), IDishRepository
{
}

