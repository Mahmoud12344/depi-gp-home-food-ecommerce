using depiR5gp.Models.Menu;

namespace depiR5gp.ViewModels;

public sealed class DishDetailsVM
{
    public Dish Dish { get; init; } = null!;
    public bool IsFavorite { get; init; }
}
