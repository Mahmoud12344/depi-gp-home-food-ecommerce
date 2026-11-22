namespace depiR5gp.Models.Chefs;

public class ChefFilterViewModel
{
    public string? Search { get; set; }
    public string? Area { get; set; }

    public void Normalize()
    {
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
        if (!PickupAreas.IsValid(Area)) Area = null;
    }
    public bool HasActiveFilters => Search != null || Area != null;
}