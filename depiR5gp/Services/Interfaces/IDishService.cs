using depiR5gp.Models.Menu;
namespace depiR5gp.Services.Interfaces;

public interface IDishService
{
    Task<DishCatalogViewModel> GetCatalogAsync(DishFilterViewModel filter);
}