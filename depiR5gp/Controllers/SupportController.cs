using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize]
public class SupportController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Index() => View();
    public IActionResult NewCase() => View();
    public IActionResult CaseDetails(int id) => View();

    [Authorize(Roles = $"{AppRoles.Support},{AppRoles.Admin}")]
    public IActionResult AgentDesk() => View();
}
