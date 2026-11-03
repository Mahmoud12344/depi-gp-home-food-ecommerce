using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class ChefDashboardController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Profile() => View();
    public IActionResult WorkingHours() => View();
    public IActionResult Documents() => View();
    public IActionResult Orders() => View();
}

