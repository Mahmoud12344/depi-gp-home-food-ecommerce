using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace depiR5gp.ViewModels.Menu;

// ── List item shown on the Index card grid ─────────────────────────────────
public class DishListItemVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }

    // Carry hidden data for Edit modal (never from form, only server-to-view)
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int CuisineId { get; set; }
}

// ── Index page view-model ───────────────────────────────────────────────────
public class MenuIndexVM
{
    public List<DishListItemVM> Dishes { get; set; } = new();

    // Filter state (for the GET filter form)
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }

    // Selects for Add modal
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Cuisines { get; set; } = new();

    public bool IsEmpty => Dishes.Count == 0;

    // ── Validation-error round-trip: reopen the correct modal ──────────────
    public bool ShowAddModal { get; set; }
    public bool ShowEditModal { get; set; }

    // Repopulate Add modal on error
    public DishCreateVM? AddFormData { get; set; }

    // Repopulate Edit modal on error
    public DishEditVM? EditFormData { get; set; }
}

// ── Edit dish view-model ────────────────────────────────────────────────────
public class DishEditVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Dish name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    // Carried as a hidden field — not shown in Edit modal, never wiped
    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 100000, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    // Carried as a hidden field — not shown in Edit modal, never wiped
    public int CuisineId { get; set; }

    public bool IsAvailable { get; set; }

    public IFormFile? ImageFile { get; set; }
    public string? ExistingImageUrl { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}

// ── Option group input for Create POST ─────────────────────────────────────
public class OptionGroupInputVM
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public List<OptionItemInputVM> Items { get; set; } = new();
}

public class OptionItemInputVM
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "Extra price cannot be negative")]
    public decimal ExtraPrice { get; set; }
}

// ── Create dish view-model ─────────────────────────────────────────────────
public class DishCreateVM
{
    [Required(ErrorMessage = "Dish name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 100000, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Cuisine is required")]
    public int CuisineId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public IFormFile? ImageFile { get; set; }

    // Option groups posted as OptionGroups[i].Name / OptionGroups[i].Items[j].Name etc.
    public List<OptionGroupInputVM> OptionGroups { get; set; } = new();

    // Select lists (re-populated on every render; not posted)
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Cuisines { get; set; } = new();
}

// ── Option form (ManageOptions page) ───────────────────────────────────────
public class DishOptionFormVM
{
    public int Id { get; set; }
    public int DishId { get; set; }

    [Required(ErrorMessage = "Group name is required")]
    [StringLength(100)]
    public string GroupName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "Extra price cannot be negative")]
    public decimal ExtraPrice { get; set; }

    public bool IsRequired { get; set; }
}

// ── Option list item (ManageOptions display) ───────────────────────────────
public class OptionItemVM
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public bool IsRequired { get; set; }
}

// ── ManageOptions page view-model ──────────────────────────────────────────
public class ManageOptionsVM
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public List<OptionItemVM> Options { get; set; } = new();
    public DishOptionFormVM NewOption { get; set; } = new();
    public bool HasOptions => Options.Count > 0;
}
