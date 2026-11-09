namespace depiR5gp.ViewModels;

public class CustomerFavoritesVM
{
    public CustomerProfileVM Profile { get; set; } = new();
    public IReadOnlyList<CustomerFavoriteVM> Favorites { get; set; } = [];
    public IReadOnlyList<CustomerDishFavoriteVM> DishFavorites { get; set; } = [];
}

public class CustomerFavoriteVM
{
    public int? ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
}

public class CustomerDishFavoriteVM
{
    public int DishId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string KitchenName { get; set; } = string.Empty;
}
