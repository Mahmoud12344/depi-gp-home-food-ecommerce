using Microsoft.AspNetCore.Mvc;
using depiR5gp.Services.Interfaces;
using depiR5gp.Models.Chefs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace depiR5gp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IChefService _chefService;

        public DashboardController(IChefService chefService)
        {
            _chefService = chefService;
        }

        public async Task<IActionResult> Index()
        {
            // جلب الـ ID الخاص بالشيف المسجل دخوله (مع تحويله لـ int ليتوافق مع مشروعك)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var chefProfile = await _chefService.GetChefApplicationStatusAsync(userId);

            if (chefProfile == null)
                return NotFound();

            var viewModel = new ChefApplicationStatusVM
            {
                Status = chefProfile.Status.ToString(), // بافتراض أن الـ Status نوعه Enum
                RejectionNotes = chefProfile.RejectionReason
            };

            return View(viewModel);
        }
    }
}