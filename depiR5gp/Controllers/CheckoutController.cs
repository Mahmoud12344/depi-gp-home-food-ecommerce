using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class CheckoutController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
}

