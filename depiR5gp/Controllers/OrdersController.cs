using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

[Authorize]
public class OrdersController(ApplicationDbContext context, UserManager<AppUser> userManager, IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Confirmation(int id) => View();

    public async Task<IActionResult> Track(int id)
    {
        AppUser? user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        Order? order = await context.Orders
            .AsNoTracking()
            .Include(item => item.ChefProfile)
            .Include(item => item.Review)
            .Include(item => item.Items)
            .ThenInclude(item => item.Options)
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == user.Id);

        return order is null ? NotFound() : View(new OrderTrackingVM { Order = order });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int orderId, string cancelReason, string? cancelNotes)
    {
        AppUser? user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        Order? order = await context.Orders
            .SingleOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

        if (order is null) return NotFound();

        if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Orders can only be cancelled before kitchen preparation starts.";
            return RedirectToAction(nameof(Track), new { id = orderId });
        }

        string reasonLabel = cancelReason switch
        {
            "changed_mind" => "Changed mind",
            "wrong_pickup" => "Selected wrong pickup slot or location",
            "ordered_by_mistake" => "Placed order by mistake",
            _ => "Customer cancelled"
        };

        if (!string.IsNullOrWhiteSpace(cancelNotes))
        {
            reasonLabel = $"{reasonLabel}: {cancelNotes.Trim()}";
        }

        order.Status = "Cancelled";
        order.CancellationReason = reasonLabel.Length > 250 ? reasonLabel[..250] : reasonLabel;
        context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            ChangedByUserId = user.Id,
            Status = "Cancelled",
            ChangedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Order #{order.Reference} has been cancelled.";
        return RedirectToAction(nameof(Track), new { id = orderId });
    }
}

