using depiR5gp.Models.Chefs;

namespace depiR5gp.Services.Interfaces
{
    public interface IChefService
    {
        Task<bool> HasPendingOrApprovedApplicationAsync(int userId);
        Task<(bool IsSuccess, string Message)> SubmitApplicationAsync(int userId, ChefApplicationViewModel model);
        Task<ChefCatalogViewModel> GetCatalogAsync(ChefFilterViewModel filter);
    }
}
