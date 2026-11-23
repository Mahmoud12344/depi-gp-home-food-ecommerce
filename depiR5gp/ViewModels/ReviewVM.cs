using System.ComponentModel.DataAnnotations;

namespace depiR5gp.ViewModels;

public class CustomerReviewsPageVM
{
    public CustomerProfileVM Profile { get; set; } = new();
    public List<PendingOrderReviewVM> PendingOrders { get; set; } = [];
    public List<CustomerSubmittedReviewVM> SubmittedReviews { get; set; } = [];
}

public class PendingOrderReviewVM
{
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? ChefPhotoUrl { get; set; }
    public decimal Total { get; set; }
    public List<ReviewedDishRefVM> Dishes { get; set; } = [];
}

public class CustomerSubmittedReviewVM
{
    public int ReviewId { get; set; }
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? ChefPhotoUrl { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public string ModerationStatus { get; set; } = "Approved";
    public DateTime CreatedAt { get; set; }
    public List<ReviewedDishRefVM> Dishes { get; set; } = [];
}

public class ReviewedDishRefVM
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ReviewFormVM
{
    public int? ReviewId { get; set; }
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int ChefProfileId { get; set; }
    public string KitchenName { get; set; } = string.Empty;
    public string? ChefPhotoUrl { get; set; }
    public List<ReviewedDishRefVM> Dishes { get; set; } = [];

    [Range(1, 5, ErrorMessage = "Please select a star rating between 1 and 5.")]
    public byte Rating { get; set; } = 5;

    [Required(ErrorMessage = "Please share a short comment about your experience.")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Your review must be between 5 and 1000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
