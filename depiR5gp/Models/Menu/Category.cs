namespace depiR5gp.Models.Menu;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation Property
    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}

