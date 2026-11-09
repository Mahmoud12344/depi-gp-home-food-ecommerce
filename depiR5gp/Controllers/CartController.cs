using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using depiR5gp.Data;
using depiR5gp.Models.Cart;
using depiR5gp.Models.Menu;
using depiR5gp.Models.Users;
using depiR5gp.ViewModels;

namespace depiR5gp.Controllers;

public class CartController(ApplicationDbContext context, UserManager<AppUser> userManager, ILogger<CartController> logger) : Controller
{
    private const string GuestCartCookie = "homechef_cart_token";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        AppUser? user = await GetCurrentUserAsync();
        List<CartItem> items = await GetOwnedItemsAsync(user?.Id, user is null ? GetGuestToken() : null);
        return View(new CartPageVM { Items = items.Select(ToViewModel).ToList() });
    }

    [HttpGet]
    public async Task<IActionResult> Count()
    {
        AppUser? user = await GetCurrentUserAsync();
        return Json(new { count = await GetCartItemCountAsync(user?.Id, user is null ? GetGuestToken() : null) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartVM model)
    {
        if (!ModelState.IsValid) return CartError("Please provide valid cart item details.");
        AppUser? user = await GetCurrentUserAsync();
        string? guestToken = GetGuestToken();
        if (user is null && guestToken is null) { guestToken = Guid.NewGuid().ToString("N"); AppendGuestCookie(guestToken); }

        Dish? dish = await context.Dishes.AsNoTracking().Include(d => d.DishOptions).SingleOrDefaultAsync(d => d.Id == model.DishId);
        if (dish is null || dish.IsArchived || !dish.IsAvailable) return CartError("This dish is currently unavailable.");

        string? note = NormalizeNote(model.Notes ?? model.SpecialInstructions);
        HashSet<int> optionIds = model.SelectedOptionIds.Where(id => id > 0).ToHashSet();
        string[] names = model.SelectedOptions.Concat(string.IsNullOrWhiteSpace(model.PortionOption) ? [] : [model.PortionOption]).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (string name in names)
        {
            DishOption? option = dish.DishOptions.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (option is null) return CartError("One or more selected options are not valid for this dish.");
            optionIds.Add(option.Id);
        }
        if (optionIds.Any(id => dish.DishOptions.All(option => option.Id != id))) return CartError("One or more selected options are not valid for this dish.");
        foreach (IGrouping<string, DishOption> group in dish.DishOptions.Where(option => option.IsRequired).GroupBy(option => option.GroupName))
        {
            if (!group.Any(option => optionIds.Contains(option.Id)))
            {
                return CartError($"Please select an option from {group.Key}.");
            }
        }

        try
        {
            if (user is not null && guestToken is not null) await MergeGuestCartAsync(user.Id, guestToken);
            Cart cart = await GetOrCreateCartAsync(user?.Id, guestToken, dish.ChefProfileId);
            string presetKey = BuildPresetKey(optionIds, model.SelectedPortion ?? model.PortionOption);
            CartItem? item = cart.Items.FirstOrDefault(x => x.DishId == dish.Id && x.PresetKey == presetKey && (x.Note ?? string.Empty) == (note ?? string.Empty));
            if (item is null)
            {
                item = new CartItem { Cart = cart, DishId = dish.Id, Quantity = model.Quantity, PresetKey = presetKey, Note = note };
                foreach (int id in optionIds.OrderBy(x => x)) item.Options.Add(new CartItemOption { DishOptionId = id });
                context.CartItems.Add(item);
            }
            else item.Quantity = checked(item.Quantity + model.Quantity);
            await context.SaveChangesAsync();
            int count = await GetCartItemCountAsync(user?.Id, user is null ? guestToken : null);
            if (user is not null && Request.Cookies.ContainsKey(GuestCartCookie)) Response.Cookies.Delete(GuestCartCookie);
            return IsAjaxRequest() ? Json(new { success = true, count, message = "Added to cart." }) : RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is OverflowException or DbUpdateException)
        {
            logger.LogError(exception, "Failed to add dish {DishId} to cart.", model.DishId);
            return CartError("We could not add this dish to your cart. Please try again.");
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
    {
        if (quantity is < 1 or > 100) return CartError("Quantity must be between 1 and 100.");
        CartItem? item = await FindOwnedItemAsync(itemId);
        if (item is null) return NotFound();
        item.Quantity = quantity;
        await context.SaveChangesAsync();

        if (IsAjaxRequest())
        {
            List<CartItem> items = await GetOwnedItemsAsync((await GetCurrentUserAsync())?.Id, GetGuestToken());
            return Json(new
            {
                success = true,
                itemId,
                quantity = item.Quantity,
                lineTotal = item.Quantity * item.Dish.Price,
                subtotal = items.Sum(cartItem => cartItem.Quantity * cartItem.Dish.Price),
                totalQuantity = items.Sum(cartItem => cartItem.Quantity),
                count = items.Sum(cartItem => cartItem.Quantity)
            });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        CartItem? item = await FindOwnedItemAsync(itemId);
        if (item is null) return NotFound();
        context.CartItems.Remove(item); await context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        AppUser? user = await GetCurrentUserAsync();
        context.CartItems.RemoveRange(await GetOwnedItemsAsync(user?.Id, user is null ? GetGuestToken() : null));
        await context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }

    private async Task<Cart> GetOrCreateCartAsync(int? userId, string? guestToken, int chefProfileId)
    {
        IQueryable<Cart> query = context.Carts.Include(x => x.Items).ThenInclude(x => x.Options).Where(x => x.ChefProfileId == chefProfileId);
        Cart? cart = userId.HasValue ? await query.SingleOrDefaultAsync(x => x.UserId == userId.Value && x.GuestToken == null) : await query.SingleOrDefaultAsync(x => x.UserId == null && x.GuestToken == guestToken);
        if (cart is not null) return cart;
        cart = new Cart { UserId = userId, GuestToken = userId.HasValue ? null : guestToken, ChefProfileId = chefProfileId };
        context.Carts.Add(cart); return cart;
    }

    private async Task MergeGuestCartAsync(int userId, string token)
    {
        List<Cart> guests = await context.Carts.Where(x => x.UserId == null && x.GuestToken == token).Include(x => x.Items).ThenInclude(x => x.Options).ToListAsync();
        List<Cart> users = await context.Carts.Where(x => x.UserId == userId && x.GuestToken == null).Include(x => x.Items).ThenInclude(x => x.Options).ToListAsync();
        foreach (Cart guest in guests)
        {
            Cart? target = users.FirstOrDefault(x => x.ChefProfileId == guest.ChefProfileId);
            if (target is null) { guest.UserId = userId; guest.GuestToken = null; users.Add(guest); continue; }
            foreach (CartItem source in guest.Items.ToList())
            {
                CartItem? match = target.Items.FirstOrDefault(x => x.DishId == source.DishId && x.PresetKey == source.PresetKey && (x.Note ?? string.Empty) == (source.Note ?? string.Empty));
                if (match is null) target.Items.Add(new CartItem { DishId = source.DishId, Quantity = source.Quantity, PresetKey = source.PresetKey, Note = source.Note, Options = source.Options.Select(x => new CartItemOption { DishOptionId = x.DishOptionId }).ToList() });
                else match.Quantity = checked(match.Quantity + source.Quantity);
                context.CartItems.Remove(source);
            }
            context.Carts.Remove(guest);
        }
    }

    private async Task<CartItem?> FindOwnedItemAsync(int id) => await OwnedQuery().SingleOrDefaultAsync(x => x.Id == id);
    private IQueryable<CartItem> OwnedQuery()
    {
        IQueryable<CartItem> query = context.CartItems.Include(x => x.Dish).Include(x => x.Cart);
        string? userId = userManager.GetUserId(User);
        return int.TryParse(userId, out int id) ? query.Where(x => x.Cart.UserId == id && x.Cart.GuestToken == null) : query.Where(x => x.Cart.UserId == null && x.Cart.GuestToken == GetGuestToken());
    }
    private async Task<List<CartItem>> GetOwnedItemsAsync(int? userId, string? token)
    {
        IQueryable<CartItem> query = context.CartItems.Include(x => x.Dish);
        query = userId.HasValue ? query.Where(x => x.Cart.UserId == userId.Value && x.Cart.GuestToken == null) : query.Where(x => x.Cart.UserId == null && x.Cart.GuestToken == token);
        return await query.OrderBy(x => x.Id).ToListAsync();
    }
    private async Task<int> GetCartItemCountAsync(int? userId, string? token)
    {
        IQueryable<CartItem> query = userId.HasValue ? context.CartItems.Where(x => x.Cart.UserId == userId.Value && x.Cart.GuestToken == null) : context.CartItems.Where(x => x.Cart.UserId == null && x.Cart.GuestToken == token);
        return await query.Select(x => (int?)x.Quantity).SumAsync() ?? 0;
    }
    private async Task<AppUser?> GetCurrentUserAsync() => await userManager.GetUserAsync(User);
    private string? GetGuestToken() => Request.Cookies.TryGetValue(GuestCartCookie, out string? token) && Guid.TryParse(token, out _) ? token : null;
    private void AppendGuestCookie(string token) => Response.Cookies.Append(GuestCartCookie, token, new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax, MaxAge = TimeSpan.FromDays(365), IsEssential = true });
    private static CartPageItemVM ToViewModel(CartItem x) => new() { Id = x.Id, Name = x.Dish.Name, ImageUrl = x.Dish.ImageUrl, UnitPrice = x.Dish.Price, Quantity = x.Quantity, Note = x.Note, PresetKey = x.PresetKey, IsAvailable = x.Dish.IsAvailable && !x.Dish.IsArchived };
    private static string? NormalizeNote(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string BuildPresetKey(IEnumerable<int> ids, string? portion) => $"options:{string.Join(',', ids.OrderBy(x => x))}|portion:{portion?.Trim().ToLowerInvariant() ?? string.Empty}";
    private IActionResult CartError(string message) => IsAjaxRequest() ? BadRequest(new { success = false, message }) : RedirectWithError(message);
    private IActionResult RedirectWithError(string message) { TempData["ErrorMessage"] = message; return RedirectToAction(nameof(Index)); }
    private bool IsAjaxRequest() => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}