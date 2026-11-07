using depiR5gp.Models.Chefs;
using depiR5gp.Models.Menu;

namespace depiR5gp.Repositories.Interfaces;

public interface IChefRepository : IGenericRepository<ChefProfile>
{
    // فحص ما إذا كان لدى المستخدم طلب قيد الانتظار أو مقبول مسبقاً
    Task<bool> HasPendingOrApprovedApplicationAsync(int userId);
    Task<List<ChefCardViewModel>> SearchAsync(ChefFilterViewModel filter);
    Task<List<FilterOption>> GetAreaOptionsAsync();
}

