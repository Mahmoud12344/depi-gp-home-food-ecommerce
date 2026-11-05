using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Orders;
using depiR5gp.Models.Users;
using depiR5gp.Services.Interfaces;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

[Authorize(Roles = AppRoles.Customer)]
public class CustomerController(
    UserManager<AppUser> userManager,
    IFileService fileService,
    ApplicationDbContext context,
    ILogger<CustomerController> logger) : Controller
{
    private const long MaxProfileImageSize = 2 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        return View(BuildViewModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        [FromForm, Bind("FullName,PhoneNumber,PrimaryArea,Address,DietaryPreferences,Allergies,ProfileImage")] CustomerProfileVM model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        model.Email = user.Email ?? string.Empty;
        model.ProfileImagePath = user.ProfileImagePath;
        model.PointsBalance = user.PointsBalance;

        if (!CustomerProfileVM.Areas.Contains(model.PrimaryArea, StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(model.PrimaryArea), "Please select a supported area.");
        }

        if (model.ProfileImage is not null)
        {
            string? imageError = await ValidateProfileImageAsync(model.ProfileImage);
            if (imageError is not null)
            {
                ModelState.AddModelError(nameof(model.ProfileImage), imageError);
            }
        }

        string fullName = model.FullName.Trim();
        string[] nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length < 2)
        {
            ModelState.AddModelError(nameof(model.FullName), "Please enter your first and last name.");
        }
        else if (nameParts[0].Length > 100 || string.Join(' ', nameParts.Skip(1)).Length > 100)
        {
            ModelState.AddModelError(nameof(model.FullName), "Each name part must be 100 characters or fewer.");
        }

        if (!ModelState.IsValid)
        {
            model.FullName = fullName;
            model.IsEditMode = true;
            return View("Profile", model);
        }

        string? previousImagePath = user.ProfileImagePath;
        string? newImagePath = null;
        string phoneNumber = model.PhoneNumber.Trim();
        string primaryArea = model.PrimaryArea.Trim();
        string address = model.Address.Trim();
        string? dietaryPreferences = string.IsNullOrWhiteSpace(model.DietaryPreferences)
            ? user.DietaryPreferences
            : model.DietaryPreferences.Trim();
        string? allergies = string.IsNullOrWhiteSpace(model.Allergies)
            ? user.Allergies
            : model.Allergies.Trim();

        bool profileChanged = !string.Equals(user.FirstName, nameParts[0], StringComparison.Ordinal)
            || !string.Equals(user.LastName, string.Join(' ', nameParts.Skip(1)), StringComparison.Ordinal)
            || !string.Equals(user.PhoneNumber ?? string.Empty, phoneNumber, StringComparison.Ordinal)
            || !string.Equals(user.PrimaryArea ?? string.Empty, primaryArea, StringComparison.Ordinal)
            || !string.Equals(user.Address ?? string.Empty, address, StringComparison.Ordinal)
            || !string.Equals(user.DietaryPreferences, dietaryPreferences, StringComparison.Ordinal)
            || !string.Equals(user.Allergies, allergies, StringComparison.Ordinal);

        if (!profileChanged && model.ProfileImage is null)
        {
            return RedirectToAction(nameof(Profile));
        }

        try
        {
            if (!string.Equals(user.FirstName, nameParts[0], StringComparison.Ordinal))
            {
                user.FirstName = nameParts[0];
            }

            string lastName = string.Join(' ', nameParts.Skip(1));
            if (!string.Equals(user.LastName, lastName, StringComparison.Ordinal))
            {
                user.LastName = lastName;
            }

            if (!string.Equals(user.PhoneNumber ?? string.Empty, phoneNumber, StringComparison.Ordinal))
            {
                user.PhoneNumber = phoneNumber;
            }

            if (!string.Equals(user.PrimaryArea ?? string.Empty, primaryArea, StringComparison.Ordinal))
            {
                user.PrimaryArea = primaryArea;
            }

            if (!string.Equals(user.Address ?? string.Empty, address, StringComparison.Ordinal))
            {
                user.Address = address;
            }

            if (!string.Equals(user.DietaryPreferences, dietaryPreferences, StringComparison.Ordinal))
            {
                user.DietaryPreferences = dietaryPreferences;
            }

            if (!string.Equals(user.Allergies, allergies, StringComparison.Ordinal))
            {
                user.Allergies = allergies;
            }

            if (model.ProfileImage is not null)
            {
                newImagePath = await fileService.SaveAsync(model.ProfileImage, "profile_images");
                user.ProfileImagePath = newImagePath;
            }

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                if (newImagePath is not null)
                {
                    await TryDeleteFileAsync(newImagePath);
                }

                model.ProfileImagePath = previousImagePath;
                model.IsEditMode = true;
                return View("Profile", model);
            }

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";

            if (newImagePath is not null && !string.Equals(previousImagePath, newImagePath, StringComparison.Ordinal))
            {
                await TryDeleteFileAsync(previousImagePath);
            }

            return RedirectToAction(nameof(Profile));
        }
        catch (Exception exception)
        {
            if (newImagePath is not null)
            {
                await TryDeleteFileAsync(newImagePath);
            }

            logger.LogError(exception, "Failed to update customer profile for user {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, "We could not save your profile right now. Please try again.");
            model.ProfileImagePath = previousImagePath;
            model.IsEditMode = true;
            return View("Profile", model);
        }
    }

    private async Task TryDeleteFileAsync(string? filePath)
    {
        try
        {
            await fileService.DeleteAsync(filePath);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not delete profile image file {FilePath}.", filePath);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var orders = await context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == user.Id)
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new CustomerOrderVM
            {
                Id = order.Id,
                Reference = order.Reference,
                CreatedAt = order.CreatedAt,
                ChefName = order.ChefProfile == null ? "Unknown chef" : order.ChefProfile.KitchenName,
                Total = order.Total,
                Status = order.Status,
                Items = order.Items
                    .OrderBy(item => item.LineNo)
                    .Select(item => new CustomerOrderItemVM
                    {
                        DishName = item.DishName,
                        Quantity = item.Quantity
                    })
                    .ToList()
            })
            .ToListAsync();

        return View(new CustomerOrdersVM
        {
            Profile = BuildViewModel(user),
            Orders = orders
        });
    }

    [HttpGet]
    public async Task<IActionResult> Favorites()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var favorites = await context.Favorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == user.Id)
            .OrderBy(favorite => favorite.ChefProfile == null ? string.Empty : favorite.ChefProfile.KitchenName)
            .Select(favorite => new CustomerFavoriteVM
            {
                ChefProfileId = favorite.ChefProfileId,
                KitchenName = favorite.ChefProfile == null ? "Chef unavailable" : favorite.ChefProfile.KitchenName,
                Bio = favorite.ChefProfile == null ? null : favorite.ChefProfile.Bio,
                PhotoUrl = favorite.ChefProfile == null ? null : favorite.ChefProfile.PhotoUrl,
                Address = favorite.ChefProfile == null ? null : favorite.ChefProfile.Address
            })
            .ToListAsync();

        return View(new CustomerFavoritesVM
        {
            Profile = BuildViewModel(user),
            Favorites = favorites
        });
    }

    [HttpGet]
    public async Task<IActionResult> Points()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var transactions = await context.PointsLedgers
            .AsNoTracking()
            .Where(entry => entry.UserId == user.Id)
            .Include(entry => entry.Order)
            .OrderByDescending(entry => entry.CreatedAt)
            .Select(entry => new CustomerPointsLedgerVM
            {
                Type = entry.Type,
                Points = entry.Points,
                CreatedAt = entry.CreatedAt,
                OrderReference = entry.Order == null ? null : entry.Order.Reference
            })
            .ToListAsync();

        return View(new CustomerPointsVM
        {
            Profile = BuildViewModel(user),
            Transactions = transactions
        });
    }

    private async Task<AppUser?> GetCurrentUserAsync()
    {
        return await userManager.GetUserAsync(User);
    }

    private static CustomerProfileVM BuildViewModel(AppUser user) => new()
    {
        UserId = user.Id,
        FullName = $"{user.FirstName} {user.LastName}".Trim(),
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        PrimaryArea = user.PrimaryArea ?? string.Empty,
        Address = user.Address ?? string.Empty,
        DietaryPreferences = user.DietaryPreferences,
        Allergies = user.Allergies,
        ProfileImagePath = user.ProfileImagePath,
        PointsBalance = user.PointsBalance
    };

    private static async Task<string?> ValidateProfileImageAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxProfileImageSize)
        {
            return "Profile pictures must be smaller than 2 MB.";
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension) || !AllowedImageContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return "Only JPG, JPEG, PNG, and WEBP images are allowed.";
        }

        byte[] header = new byte[12];
        await using Stream stream = file.OpenReadStream();
        int bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));

        bool isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        bool isPng = bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
        bool isWebp = bytesRead >= 12 && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
            && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P';

        return (extension is ".jpg" or ".jpeg" && isJpeg) || (extension == ".png" && isPng) || (extension == ".webp" && isWebp)
            ? null
            : "The uploaded file is not a valid image.";
    }
}