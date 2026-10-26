using depiR5gp.Data;
using depiR5gp.Models.Support;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class SupportRepository(ApplicationDbContext context) : GenericRepository<SupportCase>(context), ISupportRepository
{
}

