using depiR5gp.Models.Menu;
using depiR5gp.Models.ReviewsLoyalty;

namespace depiR5gp.ViewModels;

public sealed class DishDetailsVM
{
    public Dish Dish { get; init; } = null!;
    public bool IsFavorite { get; init; }
    public List<string> GalleryImages { get; init; } = [];
    public List<Review> Reviews { get; init; } = [];
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
