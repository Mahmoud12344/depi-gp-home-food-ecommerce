using depiR5gp.Data;
using depiR5gp.Models.Chefs;
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
}

