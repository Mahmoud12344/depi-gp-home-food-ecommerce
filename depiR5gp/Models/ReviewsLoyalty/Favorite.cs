using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.ReviewsLoyalty;

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ChefProfileId { get; set; }
    public int? DishId { get; set; }

    // Navigation Properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ChefProfile? ChefProfile { get; set; }
    public virtual Dish? Dish { get; set; }
}

