using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Services.Implementations;

public class DishService(IUnitOfWork unitOfWork) : IDishService
{
    public async Task<DishCatalogViewModel> GetCatalogAsync(DishFilterViewModel filter)
    {
        filter.Normalize();
        var cuisines = await unitOfWork.Dishes.GetCuisineOptionsAsync();
        var areas = await unitOfWork.Chefs.GetAreaOptionsAsync();
        foreach (var a in areas) a.Name = PickupAreas.NameOf(a.Value);

        return new DishCatalogViewModel
        {
            Filter = filter,
            Dishes = await unitOfWork.Dishes.SearchAsync(filter),
            Cuisines = cuisines,
            Areas = areas,
            MaxDishPrice = await unitOfWork.Dishes.GetMaxPriceAsync(),
            SelectedCuisineName = cuisines.FirstOrDefault(c => c.Value == filter.CuisineId?.ToString())?.Name,
            SelectedAreaName = filter.Area == null ? null : PickupAreas.NameOf(filter.Area)
        };
    }
}