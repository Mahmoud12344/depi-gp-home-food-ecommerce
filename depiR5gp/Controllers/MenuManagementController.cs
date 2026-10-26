using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class MenuManagementController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Create() => View();
    public IActionResult Edit(int id) => View();
    public IActionResult ManageOptions(int dishId) => View();
}

