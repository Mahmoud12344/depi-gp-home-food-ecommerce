using System.Security.Claims;
using depiR5gp.Data;
using depiR5gp.Models.ReviewsLoyalty;
using depiR5gp.Models.Users;
using depiR5gp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Customer)]
public class ReviewsController(ApplicationDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int orderId)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Challenge();

        var order = await context.Orders
            .AsNoTracking()
            .Include(o => o.ChefProfile)
            .Include(o => o.Items)
            .Include(o => o.Review)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction("Reviews", "Customer");
        }

        if (!string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "You can rate an order once it is completed.";
            return RedirectToAction("MyOrders", "Customer");
        }

        if (order.Review is not null)
        {
            return RedirectToAction(nameof(Edit), new { id = order.Review.Id });
        }

        return View(BuildFormVM(order, review: null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewFormVM model)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Challenge();

        var order = await context.Orders
            .Include(o => o.ChefProfile)
            .Include(o => o.Items)
            .Include(o => o.Review)
            .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.UserId == userId);

        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction("Reviews", "Customer");
        }

        if (!string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Only completed orders can be reviewed.";
            return RedirectToAction("MyOrders", "Customer");
        }

        if (order.Review is not null)
        {
            TempData["ErrorMessage"] = "You have already reviewed this order.";
            return RedirectToAction("Reviews", "Customer");
        }

        if (!ModelState.IsValid)
        {
            var hydrated = BuildFormVM(order, review: null);
            hydrated.Rating = model.Rating is >= 1 and <= 5 ? model.Rating : (byte)5;
            hydrated.Comment = model.Comment;
            return View(hydrated);
        }

        var review = new Review
        {
            OrderId = order.Id,
            UserId = userId,
            ChefProfileId = order.ChefProfileId,
            Rating = model.Rating,
            Comment = model.Comment.Trim(),
            ModerationStatus = "Approved",
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thank you! Your review has been published.";
        return RedirectToAction("Reviews", "Customer");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Challenge();

        var review = await context.Reviews
            .AsNoTracking()
            .Include(r => r.Order)
                .ThenInclude(o => o.ChefProfile)
            .Include(r => r.Order)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review is null)
        {
            TempData["ErrorMessage"] = "Review not found.";
            return RedirectToAction("Reviews", "Customer");
        }

        return View("Create", BuildFormVM(review.Order, review));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReviewFormVM model)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Challenge();

        var review = await context.Reviews
            .Include(r => r.Order)
                .ThenInclude(o => o.ChefProfile)
            .Include(r => r.Order)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review is null)
        {
            TempData["ErrorMessage"] = "Review not found.";
            return RedirectToAction("Reviews", "Customer");
        }

        if (!ModelState.IsValid)
        {
            var hydrated = BuildFormVM(review.Order, review);
            hydrated.Rating = model.Rating is >= 1 and <= 5 ? model.Rating : review.Rating;
            hydrated.Comment = model.Comment;
            return View("Create", hydrated);
        }

        review.Rating = model.Rating;
        review.Comment = model.Comment.Trim();
        if (review.ModerationStatus == "Rejected")
        {
            review.ModerationStatus = "Pending";
        }
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = review.ModerationStatus == "Pending"
            ? "Your updated review has been submitted for moderation."
            : "Your review has been updated.";
        return RedirectToAction("Reviews", "Customer");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Challenge();

        var review = await context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review is null)
        {
            TempData["ErrorMessage"] = "Review not found.";
            return RedirectToAction("Reviews", "Customer");
        }

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your review has been deleted.";
        return RedirectToAction("Reviews", "Customer");
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out int userId) ? userId : 0;
    }

    private static ReviewFormVM BuildFormVM(Models.Orders.Order order, Review? review) => new()
    {
        ReviewId = review?.Id,
        OrderId = order.Id,
        OrderReference = order.Reference,
        OrderDate = order.CreatedAt,
        ChefProfileId = order.ChefProfileId,
        KitchenName = order.ChefProfile?.KitchenName ?? "Home Kitchen",
        ChefPhotoUrl = order.ChefProfile?.PhotoUrl,
        Dishes = order.Items
            .OrderBy(i => i.LineNo)
            .Select(i => new ReviewedDishRefVM
            {
                DishId = i.DishId,
                DishName = i.DishName,
                Quantity = i.Quantity
            })
            .ToList(),
        Rating = review?.Rating ?? 5,
        Comment = review?.Comment ?? string.Empty
    };
}
