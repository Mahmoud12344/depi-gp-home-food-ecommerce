using depiR5gp.Models.Chefs;

namespace depiR5gp.Services.Interfaces
{
    public interface IChefService
    {
        Task<bool> HasPendingOrApprovedApplicationAsync(int userId);
        Task<(bool IsSuccess, string Message)> SubmitApplicationAsync(int userId, ChefApplicationViewModel model);

        // دالة لجلب بيانات الحالة للملف الشخصي للشيف بناءً على الـ UserId
        Task<ChefProfile> GetChefApplicationStatusAsync(int userId);
    }
}
