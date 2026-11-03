using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

[Authorize]
public class ChatController(IUnitOfWork unitOfWork) : Controller
{
    
    public IActionResult Index() => View();
    public IActionResult Conversation(int id) => View();
}

