using System.ComponentModel.DataAnnotations;

namespace depiR5gp.ViewModels;

public sealed class AddToCartVM
{
    [Range(1, int.MaxValue)]
    public int DishId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    public int[] SelectedOptionIds { get; set; } = [];

    public string[] SelectedOptions { get; set; } = [];

    public string? SelectedPortion { get; set; }

    public string? PortionOption { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }
}
