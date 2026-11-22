using depiR5gp.Models.Menu;

namespace depiR5gp.Repositories.Interfaces;

public interface IDishRepository : IGenericRepository<Dish>
{
    Task<List<DishCardViewModel>> SearchAsync(DishFilterViewModel filter);
    Task<List<FilterOption>> GetCuisineOptionsAsync();
    Task<decimal> GetMaxPriceAsync();
}

