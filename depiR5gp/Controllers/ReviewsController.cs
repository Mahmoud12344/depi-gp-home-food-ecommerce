using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Customer)]
public class ReviewsController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Create(int orderId) => View();
}

