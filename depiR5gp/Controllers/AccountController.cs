using Microsoft.AspNetCore.Mvc;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Controllers;

public class AccountController(IUnitOfWork unitOfWork) : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
    public IActionResult ForgotPassword() => View();
    public IActionResult ResetPassword() => View();
    public IActionResult AccessDenied() => View();
}

