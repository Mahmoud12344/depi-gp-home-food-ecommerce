using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController(IUnitOfWork unitOfWork) : Controller
{
    private static readonly string[] ValidReviewStatuses = ["Pending", "Approved", "Rejected"];

    public IActionResult Index() => View();
    public IActionResult Chefs() => View();
    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();
    public IActionResult Settings() => View();

    [HttpGet]
    public async Task<IActionResult> Reviews(string? status = "all")
    {
        string normalizedStatus = ValidReviewStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        var reviews = await unitOfWork.Reviews.GetForModerationAsync(normalizedStatus);
        ViewBag.CurrentStatus = normalizedStatus;

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reviews(int reviewId, string moderationAction, string? status = "all")
    {
        var review = await unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review is not null)
        {
            switch (moderationAction)
            {
                case "Approve":
                    review.ModerationStatus = "Approved";
                    await unitOfWork.SaveChangesAsync();
                    break;
                case "Reject":
                    review.ModerationStatus = "Rejected";
                    await unitOfWork.SaveChangesAsync();
                    break;
                case "Delete":
                    unitOfWork.Reviews.Remove(review);
                    await unitOfWork.SaveChangesAsync();
                    break;
            }
        }

        string redirectStatus = ValidReviewStatuses.FirstOrDefault(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? "all";

        return RedirectToAction(nameof(Reviews), new { status = redirectStatus });
    }
}


