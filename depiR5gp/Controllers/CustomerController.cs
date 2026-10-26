using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class CustomerController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Profile() => View();
    public IActionResult MyOrders() => View();
    public IActionResult Favorites() => View();
    public IActionResult Points() => View();
}

