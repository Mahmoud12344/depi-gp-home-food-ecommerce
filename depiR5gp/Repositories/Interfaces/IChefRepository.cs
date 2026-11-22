using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;

namespace depiR5gp.Repositories.Interfaces;

public interface IChefRepository : IGenericRepository<ChefProfile>
{
    Task<ChefProfile?> GetByUserIdAsync(int userId);
    Task<ChefProfile?> GetByIdWithDetailsAsync(int id);
    Task<List<ChefProfile>> GetApplicationsAsync(string? status = null);
    // فحص ما إذا كان لدى المستخدم طلب قيد الانتظار أو مقبول مسبقاً
    Task<bool> HasPendingOrApprovedApplicationAsync(int userId);
    Task<List<ChefCardViewModel>> SearchAsync(ChefFilterViewModel filter);
    Task<List<FilterOption>> GetAreaOptionsAsync();
}

