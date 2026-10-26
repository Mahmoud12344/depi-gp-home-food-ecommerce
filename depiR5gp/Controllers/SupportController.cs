using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class SupportController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult NewCase() => View();
    public IActionResult CaseDetails(int id) => View();
}

