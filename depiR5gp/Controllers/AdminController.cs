using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class AdminController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Chefs() => View();
    public IActionResult Users() => View();
    public IActionResult Orders() => View();
    public IActionResult Payouts() => View();
    public IActionResult Settings() => View();
}

