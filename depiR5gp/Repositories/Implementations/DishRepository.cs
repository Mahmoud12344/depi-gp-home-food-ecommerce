using depiR5gp.Data;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Repositories.Implementations;

public class DishRepository(ApplicationDbContext context) : GenericRepository<Dish>(context), IDishRepository
{

    private IQueryable<Dish> VisibleDishes() => _context.Dishes.AsNoTracking()
        .Where(d => !d.IsArchived && d.ChefProfile.Status == "Approved");

    public async Task<List<DishCardViewModel>> SearchAsync(DishFilterViewModel f)
    {
        var q = VisibleDishes();

        if (f.Search is { } s)
            q = q.Where(d => d.Name.Contains(s)
                          || (d.Description != null && d.Description.Contains(s))
                          || d.Cuisine.Name.Contains(s)
                          || d.ChefProfile.KitchenName.Contains(s));
        if (f.CuisineId != null) q = q.Where(d => d.CuisineId == f.CuisineId);
        if (f.Area != null) q = q.Where(d => d.ChefProfile.Area == f.Area);
        if (f.MinPrice != null) q = q.Where(d => d.Price >= f.MinPrice);
        if (f.MaxPrice != null) q = q.Where(d => d.Price <= f.MaxPrice);

        q = f.Sort switch
        {
            "price_asc" => q.OrderBy(d => d.Price),
            "price_desc" => q.OrderByDescending(d => d.Price),
            "rating" => q.OrderByDescending(d => d.ChefProfile.Reviews
                                .Where(r => r.ModerationStatus == "Approved").Average(r => (double?)r.Rating)),
            _ => q.OrderByDescending(d => d.Id)
        };

        return await q.Select(d => new DishCardViewModel
        {
            Id = d.Id,
            Name = d.Name,
            ImageUrl = d.ImageUrl,
            Price = d.Price,
            CuisineName = d.Cuisine.Name,
            ChefId = d.ChefProfileId,
            KitchenName = d.ChefProfile.KitchenName,
            Area = d.ChefProfile.Area,
            IsAvailable = d.IsAvailable,
            Rating = d.ChefProfile.Reviews.Where(r => r.ModerationStatus == "Approved")
                        .Average(r => (double?)r.Rating) ?? 0,
            ReviewCount = d.ChefProfile.Reviews.Count(r => r.ModerationStatus == "Approved")
        }).ToListAsync();
    }

    public async Task<List<FilterOption>> GetCuisineOptionsAsync() =>
        await VisibleDishes()
            .GroupBy(d => new { d.CuisineId, d.Cuisine.Name })
            .Select(g => new FilterOption { Value = g.Key.CuisineId.ToString(), Name = g.Key.Name, Count = g.Count() })
            .OrderBy(o => o.Name).ToListAsync();

    public async Task<decimal> GetMaxPriceAsync() =>
        await VisibleDishes().MaxAsync(d => (decimal?)d.Price) ?? 0;
}

