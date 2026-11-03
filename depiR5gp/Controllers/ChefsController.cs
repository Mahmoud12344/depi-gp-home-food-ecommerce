using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class ChefsController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Details(int id) => View();

    [Authorize(Roles = AppRoles.Chef)]
    public IActionResult Onboarding() {
        var repo = unitOfWork.Chefs;
        return View();
    }
}

