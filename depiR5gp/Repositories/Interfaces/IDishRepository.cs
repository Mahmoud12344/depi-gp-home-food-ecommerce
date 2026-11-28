using depiR5gp.Models.Menu;

namespace depiR5gp.Repositories.Interfaces;

public interface IDishRepository : IGenericRepository<Dish>
{
    Task<IEnumerable<Dish>> GetChefMenuAsync(int chefProfileId, string? search = null,
        int? categoryId = null, bool? isAvailable = null);

    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<IEnumerable<Cuisine>> GetCuisinesAsync();

    // Story 5: options & add-ons
    Task<IEnumerable<DishOption>> GetOptionsByDishIdAsync(int dishId);
    Task<DishOption?> GetOptionWithDishAsync(int optionId);
    Task<bool> IsOptionUsedInCartsAsync(int optionId);
    Task AddOptionAsync(DishOption option);
    void RemoveOption(DishOption option);
}