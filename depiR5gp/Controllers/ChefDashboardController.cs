using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class ChefDashboardController(
    ApplicationDbContext context,
    IFileService fileService) : Controller
{
    private static readonly string[] DayNames =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private static readonly string[] AllowedDocTypes = ["Identity", "HealthCertificate", "CulinaryLicense"];

    public async Task<IActionResult> Index()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        await PopulateHeaderAsync(chef);
        return View();
    }

    public IActionResult Profile() => RedirectToAction("EditProfile", "Chefs");

    public async Task<IActionResult> Orders()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        await PopulateHeaderAsync(chef);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null)
        {
            return IsAjaxRequest()
                ? NotFound(new { success = false, message = "Chef profile not found." })
                : RedirectToAction("Onboarding", "Chefs");
        }

        chef.IsAcceptingOrders = !chef.IsAcceptingOrders;
        await context.SaveChangesAsync();

        var msg = chef.IsAcceptingOrders
            ? "Your kitchen is now open and taking orders."
            : "Your kitchen is now paused for new orders.";

        if (IsAjaxRequest())
        {
            return Json(new { success = true, isAcceptingOrders = chef.IsAcceptingOrders, message = msg });
        }

        TempData["SuccessMessage"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult WorkingHours() =>
        RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkingHours(ChefWorkingHoursVM model)
    {
        var chef = await GetCurrentChefAsync(includeWorkingHours: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        var errors = new List<string>();
        var newHours = new List<ChefWorkingHours>();
        foreach (var day in model.Days)
        {
            if (day.DayOfWeek > 6) continue;
            if (!day.IsOpen) continue;

            if (!TimeOnly.TryParse(day.OpenTime, out var openTime) || !TimeOnly.TryParse(day.CloseTime, out var closeTime))
            {
                errors.Add($"Invalid open/close time for {DayNames[day.DayOfWeek]}.");
                continue;
            }

            if (openTime >= closeTime)
            {
                errors.Add($"Close time must be after open time for {DayNames[day.DayOfWeek]}.");
                continue;
            }

            newHours.Add(new ChefWorkingHours
            {
                ChefProfileId = chef.Id,
                DayOfWeek = day.DayOfWeek,
                OpenTime = openTime,
                CloseTime = closeTime
            });
        }

        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(" ", errors);
            return RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });
        }

        context.ChefWorkingHours.RemoveRange(chef.WorkingHours);
        if (newHours.Count > 0)
        {
            await context.ChefWorkingHours.AddRangeAsync(newHours);
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Kitchen working hours updated successfully.";
        return RedirectToAction("EditProfile", "Chefs", new { tab = "hours" });
    }

    [HttpGet]
    public IActionResult Documents() =>
        RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Documents(string docType, IFormFile? documentFile)
    {
        var chef = await GetCurrentChefAsync(includeDocuments: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        if (!AllowedDocTypes.Contains(docType))
        {
            TempData["ErrorMessage"] = "Invalid document type selected.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        if (documentFile is null || documentFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a document file to upload.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        if (documentFile.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Document file must be smaller than 5 MB.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        var ext = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
        if (!AllowedDocExtensions.Contains(ext))
        {
            TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed.";
            return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
        }

        var url = await fileService.SaveAsync(documentFile, "chef_documents", docType);

        var existing = chef.Documents.FirstOrDefault(d =>
            string.Equals(d.DocType, docType, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            await fileService.DeleteAsync(existing.FileUrl);
            existing.FileUrl = url;
            existing.UploadedAt = DateTime.UtcNow;
        }
        else
        {
            context.ChefDocuments.Add(new ChefDocument
            {
                ChefProfileId = chef.Id,
                DocType = docType,
                FileUrl = url,
                UploadedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{docType} document uploaded and saved successfully.";
        return RedirectToAction("EditProfile", "Chefs", new { tab = "documents" });
    }

    private async Task<ChefProfile?> GetCurrentChefAsync(bool includeWorkingHours = false, bool includeDocuments = false)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0) return null;

        IQueryable<ChefProfile> query = context.ChefProfiles;
        if (includeWorkingHours) query = query.Include(c => c.WorkingHours);
        if (includeDocuments) query = query.Include(c => c.Documents);

        return await query.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private async Task PopulateHeaderAsync(ChefProfile chef)
    {
        ViewData["ChefHeader"] = await ChefPortalHeaderVM.LoadAsync(context, chef);
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
