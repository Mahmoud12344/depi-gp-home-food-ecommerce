using depiR5gp.Models.Chefs;

namespace depiR5gp.Repositories.Interfaces;

public interface IChefRepository : IGenericRepository<ChefProfile>
{
    // فحص ما إذا كان لدى المستخدم طلب قيد الانتظار أو مقبول مسبقاً
    Task<bool> HasPendingOrApprovedApplicationAsync(int userId);
}

