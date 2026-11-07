using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Menu;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Controllers;

public class DishesController(IDishService dishService) : Controller
{
    public async Task<IActionResult> Index(DishFilterViewModel filter)
        => View(await dishService.GetCatalogAsync(filter));

    public IActionResult Details(int id) => View();
}