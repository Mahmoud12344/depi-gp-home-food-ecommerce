using System.Security.Claims;
using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Chef)]
public class MenuManagementController(
    ApplicationDbContext context,
    IFileService fileService) : Controller
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] ActiveOrderStatuses = ["Pending", "Accepted", "Preparing", "Ready"];

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");
        await PopulateHeaderAsync(chef);

        const int pageSize = 6;
        if (page < 1) page = 1;

        var baseQuery = context.Dishes
            .AsNoTracking()
            .Where(d => d.ChefProfileId == chef.Id && !d.IsArchived);

        var totalCount = await baseQuery.CountAsync();
        var availableCount = await baseQuery.CountAsync(d => d.IsAvailable);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        if (page > totalPages) page = totalPages;

        var dishes = await baseQuery
            .OrderByDescending(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new MenuDishListItemVM
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                ImageUrl = d.ImageUrl,
                CategoryName = d.Category.Name,
                CuisineName = d.Cuisine.Name,
                IsAvailable = d.IsAvailable,
                OptionsCount = d.DishOptions.Count
            })
            .ToListAsync();

        return View(new MenuIndexVM
        {
            Dishes = dishes,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            AvailableCount = availableCount,
            SoldOutCount = totalCount - availableCount
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");
        await PopulateHeaderAsync(chef);

        var model = new DishFormVM { IsAvailable = true };
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DishFormVM model)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        ValidateImageFile(model.ImageFile);
        await ValidateTaxonomyAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateHeaderAsync(chef);
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        string? imageUrl = null;
        if (model.ImageFile is { Length: > 0 })
        {
            imageUrl = await fileService.SaveAsync(model.ImageFile, "dishes", "dish");
        }

        var dish = new Dish
        {
            ChefProfileId = chef.Id,
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            Price = model.Price,
            CategoryId = model.CategoryId,
            CuisineId = model.CuisineId,
            IsAvailable = model.IsAvailable,
            IsArchived = false,
            ImageUrl = imageUrl
        };

        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"\"{dish.Name}\" has been added to your menu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        var dish = await context.Dishes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            TempData["ErrorMessage"] = "Dish not found in your kitchen menu.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateHeaderAsync(chef);

        var model = new DishFormVM
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Price = dish.Price,
            CategoryId = dish.CategoryId,
            CuisineId = dish.CuisineId,
            IsAvailable = dish.IsAvailable,
            ExistingImageUrl = dish.ImageUrl
        };
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DishFormVM model)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            TempData["ErrorMessage"] = "Dish not found in your kitchen menu.";
            return RedirectToAction(nameof(Index));
        }

        ValidateImageFile(model.ImageFile);
        await ValidateTaxonomyAsync(model);

        if (!ModelState.IsValid)
        {
            model.Id = dish.Id;
            model.ExistingImageUrl = dish.ImageUrl;
            await PopulateHeaderAsync(chef);
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        if (model.ImageFile is { Length: > 0 })
        {
            var newUrl = await fileService.SaveAsync(model.ImageFile, "dishes", "dish");
            if (!string.IsNullOrWhiteSpace(dish.ImageUrl))
            {
                await fileService.DeleteAsync(dish.ImageUrl);
            }
            dish.ImageUrl = newUrl;
        }

        dish.Name = model.Name.Trim();
        dish.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        dish.Price = model.Price;
        dish.CategoryId = model.CategoryId;
        dish.CuisineId = model.CuisineId;
        dish.IsAvailable = model.IsAvailable;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"\"{dish.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null)
        {
            return IsAjaxRequest()
                ? NotFound(new { success = false, message = "Chef profile not found." })
                : RedirectToAction("Onboarding", "Chefs");
        }
        if (chef.Status != "Approved")
        {
            return IsAjaxRequest()
                ? BadRequest(new { success = false, message = "Your kitchen must be approved first." })
                : RedirectToAction("Index", "Dashboard");
        }

        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            return IsAjaxRequest()
                ? NotFound(new { success = false, message = "Dish not found." })
                : RedirectToAction(nameof(Index));
        }

        dish.IsAvailable = !dish.IsAvailable;
        await context.SaveChangesAsync();

        var msg = dish.IsAvailable
            ? $"\"{dish.Name}\" is now marked Available."
            : $"\"{dish.Name}\" is now marked Sold Out.";

        if (IsAjaxRequest())
        {
            return Json(new { success = true, isAvailable = dish.IsAvailable, message = msg });
        }

        TempData["SuccessMessage"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            TempData["ErrorMessage"] = "Dish not found in your kitchen menu.";
            return RedirectToAction(nameof(Index));
        }

        bool hasActiveOrders = await context.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.DishId == dish.Id && ActiveOrderStatuses.Contains(oi.Order.Status));

        if (hasActiveOrders)
        {
            TempData["ErrorMessage"] = $"Cannot delete \"{dish.Name}\" because it is part of an active order. You can mark it Sold Out instead.";
            return RedirectToAction(nameof(Index));
        }

        dish.IsArchived = true;
        dish.IsAvailable = false;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"\"{dish.Name}\" has been removed from your menu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ManageOptions(int? id, int? dishId)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        int targetId = id ?? dishId ?? 0;
        if (targetId <= 0)
        {
            targetId = await context.Dishes
                .AsNoTracking()
                .Where(d => d.ChefProfileId == chef.Id && !d.IsArchived)
                .OrderByDescending(d => d.Id)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            if (targetId <= 0)
            {
                TempData["ErrorMessage"] = "Add a dish first before configuring customization options.";
                return RedirectToAction(nameof(Create));
            }
        }

        var dish = await context.Dishes
            .AsNoTracking()
            .Include(d => d.DishOptions)
            .FirstOrDefaultAsync(d => d.Id == targetId && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            TempData["ErrorMessage"] = "Dish not found in your kitchen menu.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateHeaderAsync(chef);

        var model = new ManageDishOptionsVM
        {
            DishId = dish.Id,
            DishName = dish.Name,
            BasePrice = dish.Price,
            Options = dish.DishOptions
                .OrderBy(o => o.GroupName)
                .ThenBy(o => o.Id)
                .Select(o => new DishOptionInputVM
                {
                    Id = o.Id,
                    GroupName = o.GroupName,
                    Name = o.Name,
                    ExtraPrice = o.ExtraPrice,
                    IsRequired = o.IsRequired
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageOptions(int id, ManageDishOptionsVM model)
    {
        var chef = await GetCurrentChefAsync();
        if (chef is null) return RedirectToAction("Onboarding", "Chefs");
        if (chef.Status != "Approved") return RedirectToAction("Index", "Dashboard");

        int targetId = id > 0 ? id : model.DishId;
        var dish = await context.Dishes
            .Include(d => d.DishOptions)
            .FirstOrDefaultAsync(d => d.Id == targetId && d.ChefProfileId == chef.Id && !d.IsArchived);

        if (dish is null)
        {
            TempData["ErrorMessage"] = "Dish not found in your kitchen menu.";
            return RedirectToAction(nameof(Index));
        }

        var submitted = (model.Options ?? [])
            .Where(o => !string.IsNullOrWhiteSpace(o.GroupName) || !string.IsNullOrWhiteSpace(o.Name))
            .ToList();

        for (int i = 0; i < submitted.Count; i++)
        {
            var opt = submitted[i];
            if (string.IsNullOrWhiteSpace(opt.GroupName) || string.IsNullOrWhiteSpace(opt.Name))
            {
                ModelState.AddModelError(string.Empty, $"Option #{i + 1} must have both a Group Name and an Option Name.");
            }
            if (opt.ExtraPrice < 0 || opt.ExtraPrice > 10000)
            {
                ModelState.AddModelError(string.Empty, $"Extra price for \"{opt.Name}\" must be between 0 and 10,000 EGP.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.DishId = dish.Id;
            model.DishName = dish.Name;
            model.BasePrice = dish.Price;
            model.Options = submitted;
            await PopulateHeaderAsync(chef);
            return View(model);
        }

        var submittedIds = submitted.Where(o => o.Id > 0).Select(o => o.Id).ToHashSet();
        var toDelete = dish.DishOptions.Where(existing => !submittedIds.Contains(existing.Id)).ToList();

        if (toDelete.Count > 0)
        {
            var deleteIds = toDelete.Select(o => o.Id).ToList();
            var cartOptionRefs = await context.CartItemOptions
                .Where(co => deleteIds.Contains(co.DishOptionId))
                .ToListAsync();
            if (cartOptionRefs.Count > 0)
            {
                context.CartItemOptions.RemoveRange(cartOptionRefs);
            }
            context.DishOptions.RemoveRange(toDelete);
        }

        foreach (var opt in submitted)
        {
            if (opt.Id > 0)
            {
                var existing = dish.DishOptions.FirstOrDefault(x => x.Id == opt.Id);
                if (existing is not null)
                {
                    existing.GroupName = opt.GroupName.Trim();
                    existing.Name = opt.Name.Trim();
                    existing.ExtraPrice = opt.ExtraPrice;
                    existing.IsRequired = opt.IsRequired;
                    continue;
                }
            }

            dish.DishOptions.Add(new DishOption
            {
                DishId = dish.Id,
                GroupName = opt.GroupName.Trim(),
                Name = opt.Name.Trim(),
                ExtraPrice = opt.ExtraPrice,
                IsRequired = opt.IsRequired
            });
        }

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Customization options saved for \"{dish.Name}\".";
        return RedirectToAction(nameof(ManageOptions), new { id = dish.Id });
    }

    private void ValidateImageFile(IFormFile? imageFile)
    {
        if (imageFile is null || imageFile.Length == 0) return;

        if (imageFile.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(DishFormVM.ImageFile), "Dish image must be smaller than 5 MB.");
            return;
        }

        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
        {
            ModelState.AddModelError(nameof(DishFormVM.ImageFile), "Only JPG, JPEG, PNG, and WEBP images are allowed.");
        }
    }

    private async Task ValidateTaxonomyAsync(DishFormVM model)
    {
        if (!await context.Categories.AsNoTracking().AnyAsync(c => c.Id == model.CategoryId))
        {
            ModelState.AddModelError(nameof(DishFormVM.CategoryId), "Please select a valid category.");
        }
        if (!await context.Cuisines.AsNoTracking().AnyAsync(c => c.Id == model.CuisineId))
        {
            ModelState.AddModelError(nameof(DishFormVM.CuisineId), "Please select a valid cuisine.");
        }
    }

    private async Task PopulateDropdownsAsync(DishFormVM model)
    {
        model.Categories = await context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        model.Cuisines = await context.Cuisines.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    private async Task<ChefProfile?> GetCurrentChefAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0) return null;
        return await context.ChefProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private async Task PopulateHeaderAsync(ChefProfile chef)
    {
        ViewData["ChefHeader"] = await ChefPortalHeaderVM.LoadAsync(context, chef);
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
