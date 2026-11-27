using depiR5gp.Services;
using Microsoft.AspNetCore.Mvc;

namespace depiR5gp.Controllers;

public class ChefProfileController : Controller
{
    private readonly IChefProfileService _chefProfileService;

    public ChefProfileController(IChefProfileService chefProfileService)
    {
        _chefProfileService = chefProfileService;
    }

    // الـ id هنا يمثل UserId الخاص بالطباخ
    public async Task<IActionResult> Details(int id)
    {
        var viewModel = await _chefProfileService.GetChefProfileAsync(id);

        if (viewModel == null)
        {
            // إذا لم يتم العثور على الطباخ أو حسابه غير مقبول
            return NotFound("Chef profile not found or unavailable.");
        }

        return View(viewModel);
    }
}