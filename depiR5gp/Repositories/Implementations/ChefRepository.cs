using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class ChefRepository(ApplicationDbContext context) : GenericRepository<ChefProfile>(context), IChefRepository
{
}

