using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class ReviewsController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Create(int orderId) => View();
}

