using depiR5gp.Data;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Repositories.Implementations;

public class ReviewRepository(ApplicationDbContext context) : GenericRepository<Review>(context), IReviewRepository
{
}

