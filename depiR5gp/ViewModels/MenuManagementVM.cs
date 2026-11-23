using System.ComponentModel.DataAnnotations;
using depiR5gp.Models.Menu;
using Microsoft.AspNetCore.Http;

namespace depiR5gp.ViewModels;

public class MenuDishListItemVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CuisineName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int OptionsCount { get; set; }
}

public class MenuIndexVM
{
    public List<MenuDishListItemVM> Dishes { get; set; } = [];
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalCount { get; set; }
    public int AvailableCount { get; set; }
    public int SoldOutCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
}

public class DishFormVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Dish name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Dish name must be between 2 and 120 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(1, 50000, ErrorMessage = "Price must be between 1 and 50,000 EGP.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please select a cuisine.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid cuisine.")]
    public int CuisineId { get; set; }

    public bool IsAvailable { get; set; } = true;
    public string? ExistingImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }

    public List<Category> Categories { get; set; } = [];
    public List<Cuisine> Cuisines { get; set; } = [];
}

public class DishOptionInputVM
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public bool IsRequired { get; set; }
}

public class ManageDishOptionsVM
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public List<DishOptionInputVM> Options { get; set; } = [];
}
