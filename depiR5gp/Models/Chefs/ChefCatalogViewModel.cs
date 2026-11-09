using depiR5gp.Models.Menu;
namespace depiR5gp.Models.Chefs;

public class ChefCatalogViewModel
{
    public ChefFilterViewModel Filter { get; set; } = new();
    public List<ChefCardViewModel> Chefs { get; set; } = new();
    public List<FilterOption> Areas { get; set; } = new();
}