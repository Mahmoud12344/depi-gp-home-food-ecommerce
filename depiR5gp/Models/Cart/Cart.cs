using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;

namespace depiR5gp.Models.Cart;

public class Cart
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ChefProfileId { get; set; }
    public string? GuestToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual AppUser? User { get; set; }
    public virtual ChefProfile ChefProfile { get; set; } = null!;
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

