namespace depiR5gp.ViewModels;

public class CustomerFavoritesVM
{
    public CustomerProfileVM Profile { get; set; } = new();
    public IReadOnlyList<CustomerFavoriteVM> Favorites { get; set; } = [];
}

public class CustomerFavoriteVM
{
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
}
