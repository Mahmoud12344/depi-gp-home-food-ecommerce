using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class MenuManagementController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Edit(int id) => View();
    public IActionResult ManageOptions(int dishId) => View();
}

