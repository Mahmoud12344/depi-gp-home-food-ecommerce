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
    public async Task<IActionResult> WorkingHours()
    {
        var chef = await GetCurrentChefAsync(includeWorkingHours: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        await PopulateHeaderAsync(chef);

        var existingByDay = chef.WorkingHours.ToDictionary(w => w.DayOfWeek);
        var model = new ChefWorkingHoursVM
        {
            Days = Enumerable.Range(0, 7).Select(i =>
            {
                byte day = (byte)i;
                if (existingByDay.TryGetValue(day, out var hours))
                {
                    return new ChefWorkingDayVM
                    {
                        DayOfWeek = day,
                        DayName = DayNames[i],
                        IsOpen = true,
                        OpenTime = hours.OpenTime.ToString("HH:mm"),
                        CloseTime = hours.CloseTime.ToString("HH:mm")
                    };
                }

                return new ChefWorkingDayVM
                {
                    DayOfWeek = day,
                    DayName = DayNames[i],
                    IsOpen = false,
                    OpenTime = "12:00",
                    CloseTime = "22:00"
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkingHours(ChefWorkingHoursVM model)
    {
        var chef = await GetCurrentChefAsync(includeWorkingHours: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        var newHours = new List<ChefWorkingHours>();
        foreach (var day in model.Days)
        {
            if (day.DayOfWeek > 6) continue;
            if (!day.IsOpen) continue;

            if (!TimeOnly.TryParse(day.OpenTime, out var openTime) || !TimeOnly.TryParse(day.CloseTime, out var closeTime))
            {
                ModelState.AddModelError(string.Empty, $"Invalid open/close time for {DayNames[day.DayOfWeek]}.");
                continue;
            }

            if (openTime >= closeTime)
            {
                ModelState.AddModelError(string.Empty, $"Close time must be after open time for {DayNames[day.DayOfWeek]}.");
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

        if (!ModelState.IsValid)
        {
            foreach (var d in model.Days)
            {
                if (d.DayOfWeek <= 6) d.DayName = DayNames[d.DayOfWeek];
            }
            await PopulateHeaderAsync(chef);
            return View(model);
        }

        context.ChefWorkingHours.RemoveRange(chef.WorkingHours);
        if (newHours.Count > 0)
        {
            await context.ChefWorkingHours.AddRangeAsync(newHours);
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Kitchen working hours updated successfully.";
        return RedirectToAction(nameof(WorkingHours));
    }

    [HttpGet]
    public async Task<IActionResult> Documents()
    {
        var chef = await GetCurrentChefAsync(includeDocuments: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        await PopulateHeaderAsync(chef);

        var model = new ChefDocumentsVM
        {
            KitchenName = chef.KitchenName,
            Status = chef.Status,
            Documents = chef.Documents.OrderByDescending(d => d.UploadedAt).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Documents(string docType, IFormFile? documentFile)
    {
        var chef = await GetCurrentChefAsync(includeDocuments: true);
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");

        if (!AllowedDocTypes.Contains(docType))
        {
            TempData["ErrorMessage"] = "Invalid document type selected.";
            return RedirectToAction(nameof(Documents));
        }

        if (documentFile is null || documentFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a document file to upload.";
            return RedirectToAction(nameof(Documents));
        }

        if (documentFile.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Document file must be smaller than 5 MB.";
            return RedirectToAction(nameof(Documents));
        }

        var ext = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
        if (!AllowedDocExtensions.Contains(ext))
        {
            TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed.";
            return RedirectToAction(nameof(Documents));
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
        return RedirectToAction(nameof(Documents));
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
