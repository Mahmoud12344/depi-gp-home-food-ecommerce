using depiR5gp.Models.ReviewsLoyalty;

namespace depiR5gp.Repositories.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetForModerationAsync(string? status = null);
}


