using depiR5gp.Data;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Repositories.Implementations;

public class DishRepository(ApplicationDbContext context) : GenericRepository<Dish>(context), IDishRepository
{
    public async Task<IEnumerable<Dish>> GetChefMenuAsync(int chefProfileId, string? search = null,
        int? categoryId = null, bool? isAvailable = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(d => d.Category)
            .Where(d => d.ChefProfileId == chefProfileId && !d.IsArchived);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));
        if (categoryId.HasValue)
            query = query.Where(d => d.CategoryId == categoryId.Value);
        if (isAvailable.HasValue)
            query = query.Where(d => d.IsAvailable == isAvailable.Value);

        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync() =>
        await _context.Set<Category>().AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public async Task<IEnumerable<Cuisine>> GetCuisinesAsync() =>
        await _context.Set<Cuisine>().AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    // ---------- Story 5: options & add-ons ----------

    public async Task<IEnumerable<DishOption>> GetOptionsByDishIdAsync(int dishId) =>
        await _context.Set<DishOption>()
            .AsNoTracking()
            .Where(o => o.DishId == dishId)
            .OrderBy(o => o.GroupName).ThenBy(o => o.Name)
            .ToListAsync();

    public async Task<DishOption?> GetOptionWithDishAsync(int optionId) =>
        await _context.Set<DishOption>()
            .Include(o => o.Dish)
            .FirstOrDefaultAsync(o => o.Id == optionId);

    public async Task<bool> IsOptionUsedInCartsAsync(int optionId) =>
        await _context.Set<DishOption>()
            .Where(o => o.Id == optionId)
            .SelectMany(o => o.CartItemOptions)
            .AnyAsync();

    public async Task AddOptionAsync(DishOption option) =>
        await _context.Set<DishOption>().AddAsync(option);

    public void RemoveOption(DishOption option) =>
        _context.Set<DishOption>().Remove(option);
}