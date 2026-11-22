using depiR5gp.Models.Chefs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace depiR5gp.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<ChefProfile>> GetChefApplicationsAsync(string? status = null);
        Task<ChefProfile?> GetChefApplicationAsync(int id);
        Task<bool> ApproveApplicationAsync(int id, string? approvalNotes);
        Task<bool> RejectApplicationAsync(int id, string rejectionReason);
    }
}