using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class OrdersController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Confirmation(int id) => View();
    public IActionResult Track(int id) => View();
}

