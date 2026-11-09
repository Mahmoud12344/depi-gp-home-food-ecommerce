using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace depiR5gp.ViewModels;

public class CustomerProfileVM
{
    public int UserId { get; set; }

    [Display(Name = "Full name")]
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(201, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Required(ErrorMessage = "Please enter your phone number.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Primary area")]
    [Required(ErrorMessage = "Please select your primary area.")]
    public string PrimaryArea { get; set; } = string.Empty;

    [Display(Name = "Street and building details")]
    [Required(ErrorMessage = "Please enter your street and building details.")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Dietary preferences")]
    [StringLength(1000)]
    public string? DietaryPreferences { get; set; }

    [Display(Name = "Allergies")]
    [StringLength(1000)]
    public string? Allergies { get; set; }

    public string? ProfileImagePath { get; set; }
    public int PointsBalance { get; set; }
    public decimal PointsDiscount => PointsBalance / 5m;
    public bool IsEditMode { get; set; }

    [Display(Name = "Profile picture")]
    public IFormFile? ProfileImage { get; set; }

    public static IReadOnlyList<string> Areas { get; } =
    [
        "Maadi, Cairo",
        "Zamalek, Cairo",
        "Nasr City, Cairo",
        "Heliopolis, Cairo",
        "Dokki & Mohandessin",
        "New Cairo / 5th Settlement"
    ];
}