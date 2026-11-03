using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Services;

public class ChefProfileService : IChefProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChefProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChefProfileViewModel?> GetChefProfileAsync(int userId)
    {
        // استخدام Chefs هنا بناءً على افتراض أن هذا هو اسمها داخل IUnitOfWork
        var profile = await _unitOfWork.Chefs.GetByUserIdAsync(userId);

        // إذا لم نجد الطباخ أو حسابه غير مقبول، نرجع null
        if (profile == null || profile.Status != "Approved")
        {
            return null;
        }

        return new ChefProfileViewModel
        {
            Name = !string.IsNullOrEmpty(profile.KitchenName) ? profile.KitchenName : (profile.User?.FirstName + " " + profile.User?.LastName ?? "Unknown"),
            Bio = profile.Bio,
            PhotoUrl = profile.PhotoUrl,
            Address = profile.Address,
            // تنسيق مواعيد العمل لتظهر كنصوص
            WorkingHours = profile.WorkingHours.Select(w => $"{w.DayOfWeek}: {w.OpenTime} - {w.CloseTime}").ToList(),
            IsAcceptingOrders = profile.IsAcceptingOrders
        };
    }
}