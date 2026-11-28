using System.Security.Claims;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.ViewModels.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace depiR5gp.Controllers;

//[Authorize(Roles = "Chef")]
public class MenuManagementController(IUnitOfWork uow, IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxImageBytes = 2 * 1024 * 1024; // 2MB

    // ---------- Helpers ----------

    private async Task<ChefProfile?> GetCurrentChefAsync()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var userId)) return null;
        var chefs = await uow.Chefs.FindAsync(c => c.UserId == userId);
        return chefs.FirstOrDefault();
    }

    private static List<SelectListItem> ToItems(IEnumerable<Category> categories) =>
        categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

    private string? ValidateImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) return "Only jpg, jpeg, png, webp images are allowed";
        if (file.Length > MaxImageBytes) return "Image size must not exceed 2MB";
        return null;
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var folder = Path.Combine(env.WebRootPath, "images", "dishes");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/images/dishes/{fileName}";
    }

    private void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        var path = Path.Combine(env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    // ---------- Build Index VM helper ----------

    private async Task<MenuIndexVM> BuildIndexVMAsync(string? search, int? categoryId, bool? isAvailable)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return new MenuIndexVM();
        var dishes = await uow.Dishes.GetChefMenuAsync(chef.Id, search, categoryId, isAvailable);
        var categories = ToItems(await uow.Dishes.GetCategoriesAsync());
        var cuisines = (await uow.Dishes.GetCuisinesAsync())
            .Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

        return new MenuIndexVM
        {
            Search = search,
            CategoryId = categoryId,
            IsAvailable = isAvailable,
            Dishes = dishes.Select(d => new DishListItemVM
            {
                Id = d.Id,
                Name = d.Name,
                ImageUrl = d.ImageUrl,
                Price = d.Price,
                CategoryName = d.Category.Name,
                IsAvailable = d.IsAvailable,
                Description = d.Description,
                CategoryId = d.CategoryId,
                CuisineId = d.CuisineId
            }).ToList(),
            Categories = categories,
            Cuisines = cuisines
        };
    }

    // ---------- Story 1: View menu ----------

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? categoryId, bool? isAvailable)
    {
       // var chef = await GetCurrentChefAsync();
        //if (chef == null) return Forbid();

        var vm = await BuildIndexVMAsync(search, categoryId, isAvailable);
        return View(vm);
    }

    // ---------- Story 2: Edit dish ----------

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(id);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 3

        // GET Edit redirects to Index; the Edit modal is on Index.
        // We redirect with a flag so the Index can open the edit modal.
        // TempData carries the edit vm (dish id only — the modal fills via data-* attributes).
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DishEditVM vm)
    {
        if (id != vm.Id) return BadRequest();

        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(id);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 3

        var categories = (await uow.Dishes.GetCategoriesAsync()).ToList();
        if (!categories.Any(c => c.Id == vm.CategoryId))
            ModelState.AddModelError(nameof(vm.CategoryId), "Invalid category");

        if (vm.ImageFile != null)
        {
            var error = ValidateImage(vm.ImageFile);
            if (error != null) ModelState.AddModelError(nameof(vm.ImageFile), error);
        }

        if (!ModelState.IsValid)   // Scenario 2: nothing is saved
        {
            vm.ExistingImageUrl = dish.ImageUrl;
            vm.Categories = ToItems(categories);

            // Reopen Index with the Edit modal showing errors
            var indexVm = await BuildIndexVMAsync(null, null, null);
            indexVm.ShowEditModal = true;
            indexVm.EditFormData = vm;
            return View("Index", indexVm);
        }

        // Scenario 1 + 4: only these fields change.
        // Description and CuisineId are carried as hidden inputs — never wiped.
        dish.Name = vm.Name.Trim();
        dish.Description = vm.Description?.Trim();   // from hidden input
        dish.Price = vm.Price;
        dish.CategoryId = vm.CategoryId;
        dish.IsAvailable = vm.IsAvailable;
        // CuisineId carried as hidden input — apply it here too so it is never wiped
        if (vm.CuisineId > 0) dish.CuisineId = vm.CuisineId;

        string? oldImage = null;
        if (vm.ImageFile != null)
        {
            oldImage = dish.ImageUrl;
            dish.ImageUrl = await SaveImageAsync(vm.ImageFile);
        }

        await uow.CompleteAsync();
        DeleteImage(oldImage);   // delete the old image only after a successful save

        TempData["Success"] = "Dish updated successfully";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Story 3: Delete (soft delete / archive) ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(id);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 3

        // Archive instead of removing the row: order history stays intact (Scenario 4)
        dish.IsArchived = true;
        dish.IsAvailable = false;
        await uow.CompleteAsync();

        TempData["Success"] = "Dish removed from your menu";   // Scenario 1
        return RedirectToAction(nameof(Index));
    }

    // ---------- Story 4: Manage availability ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(id);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 3

        // Only flips the flag. Existing orders are not touched (Scenario 4)
        dish.IsAvailable = !dish.IsAvailable;                 // Scenarios 1 and 2
        await uow.CompleteAsync();

        TempData["Success"] = dish.IsAvailable
            ? "Dish is now available"
            : "Dish is now unavailable";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Story 5: Options & Add-ons ----------

    private async Task<ManageOptionsVM> BuildOptionsVMAsync(Dish dish, DishOptionFormVM? newOption = null)
    {
        var options = await uow.Dishes.GetOptionsByDishIdAsync(dish.Id);
        return new ManageOptionsVM
        {
            DishId = dish.Id,
            DishName = dish.Name,
            Options = options.Select(o => new OptionItemVM
            {
                Id = o.Id,
                GroupName = o.GroupName,
                Name = o.Name,
                ExtraPrice = o.ExtraPrice,
                IsRequired = o.IsRequired
            }).ToList(),
            NewOption = newOption ?? new DishOptionFormVM { DishId = dish.Id }
        };
    }

    [HttpGet]
    public async Task<IActionResult> ManageOptions(int dishId)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(dishId);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 4

        return View(await BuildOptionsVMAsync(dish));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOption(DishOptionFormVM vm)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var dish = await uow.Dishes.GetByIdAsync(vm.DishId);
        if (dish == null || dish.IsArchived) return NotFound();
        if (dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 4

        if (!ModelState.IsValid)                              // Scenario 3
            return View("ManageOptions", await BuildOptionsVMAsync(dish, vm));

        await uow.Dishes.AddOptionAsync(new DishOption
        {
            DishId = dish.Id,                                 // from the loaded dish, not from the form
            GroupName = vm.GroupName.Trim(),
            Name = vm.Name.Trim(),
            ExtraPrice = vm.ExtraPrice,                       // Scenario 2
            IsRequired = vm.IsRequired
        });
        await uow.CompleteAsync();

        TempData["Success"] = "Option added";                 // Scenario 1
        return RedirectToAction(nameof(ManageOptions), new { dishId = dish.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOption(DishOptionFormVM vm)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var option = await uow.Dishes.GetOptionWithDishAsync(vm.Id);
        if (option == null) return NotFound();
        if (option.Dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 4

        if (!ModelState.IsValid)                                     // Scenario 3
        {
            TempData["Error"] = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(ManageOptions), new { dishId = option.DishId });
        }

        option.GroupName = vm.GroupName.Trim();
        option.Name = vm.Name.Trim();
        option.ExtraPrice = vm.ExtraPrice;
        option.IsRequired = vm.IsRequired;
        await uow.CompleteAsync();

        TempData["Success"] = "Option updated";                      // Scenario 5
        return RedirectToAction(nameof(ManageOptions), new { dishId = option.DishId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOption(int id)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();

        var option = await uow.Dishes.GetOptionWithDishAsync(id);
        if (option == null) return NotFound();
        if (option.Dish.ChefProfileId != chef.Id) return Forbid();   // Scenario 4

        // Don't break carts that reference this option (Scenario 5)
        if (await uow.Dishes.IsOptionUsedInCartsAsync(id))
        {
            TempData["Error"] = "This option is used in existing carts and can't be deleted";
            return RedirectToAction(nameof(ManageOptions), new { dishId = option.DishId });
        }

        var dishId = option.DishId;
        uow.Dishes.RemoveOption(option);
        await uow.CompleteAsync();

        TempData["Success"] = "Option deleted";
        return RedirectToAction(nameof(ManageOptions), new { dishId });
    }

    // ---------- Story 6: Add dish ----------

    private async Task FillCreateListsAsync(DishCreateVM vm)
    {
        vm.Categories = ToItems(await uow.Dishes.GetCategoriesAsync());
        vm.Cuisines = (await uow.Dishes.GetCuisinesAsync())
            .Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
      //  var chef = await GetCurrentChefAsync();
      //  if (chef == null) return Forbid();

        // The Add modal lives on Index; redirect there.
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DishCreateVM vm)
    {
        var chef = await GetCurrentChefAsync();
        if (chef == null) return Forbid();   // Scenario 4

        await FillCreateListsAsync(vm);

        if (!vm.Categories.Any(c => c.Value == vm.CategoryId.ToString()))
            ModelState.AddModelError(nameof(vm.CategoryId), "Invalid category");
        if (!vm.Cuisines.Any(c => c.Value == vm.CuisineId.ToString()))
            ModelState.AddModelError(nameof(vm.CuisineId), "Invalid cuisine");

        if (vm.ImageFile != null)
        {
            var error = ValidateImage(vm.ImageFile);
            if (error != null) ModelState.AddModelError(nameof(vm.ImageFile), error);
        }

        if (!ModelState.IsValid)   // Scenario 2: nothing is saved
        {
            var indexVm = await BuildIndexVMAsync(null, null, null);
            indexVm.ShowAddModal = true;
            indexVm.AddFormData = vm;
            return View("Index", indexVm);
        }

        var dish = new Dish
        {
            ChefProfileId = chef.Id,   // from the logged-in chef, never from the form
            Name = vm.Name.Trim(),
            Description = vm.Description.Trim(),
            Price = vm.Price,
            CategoryId = vm.CategoryId,
            CuisineId = vm.CuisineId,
            IsAvailable = vm.IsAvailable
        };

        if (vm.ImageFile != null)      // Scenario 3
            dish.ImageUrl = await SaveImageAsync(vm.ImageFile);

        await uow.Dishes.AddAsync(dish);

        // Save dish + all option rows in ONE CompleteAsync call (Story 6)
        foreach (var group in vm.OptionGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Name)) continue;
            foreach (var item in group.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;
                await uow.Dishes.AddOptionAsync(new DishOption
                {
                    Dish = dish,                        // navigation property links to the new dish
                    GroupName = group.Name.Trim(),
                    Name = item.Name.Trim(),
                    ExtraPrice = item.ExtraPrice,
                    IsRequired = group.IsRequired       // copied from the group-level checkbox
                });
            }
        }

        await uow.CompleteAsync();                      // ONE save for dish + all option rows

        TempData["Success"] = "Dish added successfully";   // Scenario 1
        return RedirectToAction(nameof(Index));
    }
}
