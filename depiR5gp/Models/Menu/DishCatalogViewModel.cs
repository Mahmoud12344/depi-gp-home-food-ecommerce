namespace depiR5gp.Models.Menu;

public class DishCatalogViewModel
{
    public DishFilterViewModel Filter { get; set; } = new();
    public List<DishCardViewModel> Dishes { get; set; } = new();
    public List<FilterOption> Cuisines { get; set; } = new();
    public List<FilterOption> Areas { get; set; } = new();
    public decimal MaxDishPrice { get; set; }
    public string? SelectedCuisineName { get; set; }
    public string? SelectedAreaName { get; set; }
}