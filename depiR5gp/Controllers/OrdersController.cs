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
            .Include(item => item.Items)
            .ThenInclude(item => item.Options)
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == user.Id);

        return order is null ? NotFound() : View(new OrderTrackingVM { Order = order });
    }
}

