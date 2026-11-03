using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using depiR5gp.ViewModels;
using depiR5gp.Models.Users;

namespace depiR5gp.Controllers;

public class AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    RoleManager<IdentityRole<int>> roleManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM loginVm)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", loginVm);
        }

        string identifier = loginVm.Email.Trim();

        AppUser? user = await userManager.FindByEmailAsync(identifier)
                        ?? await userManager.FindByNameAsync(identifier);

        if (user is null || user.IsDeleted)
        {
            ModelState.AddModelError(string.Empty, "Invalid email/username or password.");
            return View("Login", loginVm);
        }

        if (user.IsSuspended)
        {
            ModelState.AddModelError(string.Empty, "Your account has been suspended. Please contact support.");
            return View("Login", loginVm);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            loginVm.Password,
            loginVm.RememberMe,
            lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";

            if (!string.IsNullOrWhiteSpace(loginVm.ReturnUrl) && Url.IsLocalUrl(loginVm.ReturnUrl))
            {
                return LocalRedirect(loginVm.ReturnUrl);
            }

            if (await userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (await userManager.IsInRoleAsync(user, AppRoles.Chef))
            {
                return RedirectToAction("Index", "ChefDashboard");
            }

            if (await userManager.IsInRoleAsync(user, AppRoles.Support))
            {
                return RedirectToAction("AgentDesk", "Support");
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid email/username or password.");
        return View("Login", loginVm);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM userVm)
    {
        if (!ModelState.IsValid)
        {
            return View("Register", userVm);
        }

        string assignedRole = string.Equals(userVm.Role?.Trim(), AppRoles.Chef, StringComparison.OrdinalIgnoreCase)
            ? AppRoles.Chef
            : AppRoles.Customer;

        string normalizedUserName = userVm.UserName.Trim();
        string normalizedEmail = userVm.Email.Trim();
        string normalizedPhone = userVm.PhoneNumber.Trim();

        var existingUserByName = await userManager.FindByNameAsync(normalizedUserName);
        if (existingUserByName is not null)
        {
            ModelState.AddModelError(nameof(userVm.UserName), "This username is already taken.");
        }

        var existingEmailUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingEmailUser is not null)
        {
            ModelState.AddModelError(nameof(userVm.Email), "An account with this email address already exists.");
        }

        bool phoneExists = await userManager.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone);
        if (phoneExists)
        {
            ModelState.AddModelError(nameof(userVm.PhoneNumber), "An account with this phone number already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View("Register", userVm);
        }

        AppUser user = new AppUser
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone,
            FirstName = userVm.FirstName.Trim(),
            LastName = userVm.LastName.Trim(),
            Address = userVm.Address?.Trim()
        };

        IdentityResult result = await userManager.CreateAsync(user, userVm.Password);
        if (result.Succeeded)
        {
            if (!await roleManager.RoleExistsAsync(assignedRole))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(assignedRole));
            }

            IdentityResult roleResult = await userManager.AddToRoleAsync(user, assignedRole);
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Register", userVm);
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Your account has been created successfully!";

            if (assignedRole == AppRoles.Chef)
            {
                return RedirectToAction("Onboarding", "Chefs");
            }

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Register", userVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        TempData["InfoMessage"] = "You have been signed out.";
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutGet()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public IActionResult ForgotPassword() => View();
    public IActionResult ResetPassword() => View();
    public IActionResult AccessDenied() => View();
}
