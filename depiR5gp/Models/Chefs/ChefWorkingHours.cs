namespace depiR5gp.Models.Chefs;

public class ChefWorkingHours
{
    public int ChefProfileId { get; set; }
    public byte DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }

    // Navigation Property
    public virtual ChefProfile ChefProfile { get; set; } = null!;
}

