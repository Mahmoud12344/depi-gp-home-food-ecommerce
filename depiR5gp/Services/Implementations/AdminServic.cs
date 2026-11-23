using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace depiR5gp.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChefProfile>> GetChefApplicationsAsync(string? status = null)
        {
            return await _unitOfWork.Chefs.GetApplicationsAsync(status);
        }

        public async Task<ChefProfile?> GetChefApplicationAsync(int id)
        {
            return await _unitOfWork.Chefs.GetByIdWithDetailsAsync(id);
        }

        public async Task<bool> ApproveApplicationAsync(int id, string? approvalNotes)
        {
            var chefProfile = await _unitOfWork.Chefs.GetByIdAsync(id);
            if (chefProfile == null || chefProfile.Status != "Pending") return false;

            chefProfile.Status = "Approved";
            chefProfile.ApprovalNotes = approvalNotes?.Trim();

            _unitOfWork.Chefs.Update(chefProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectApplicationAsync(int id, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason)) return false;

            var chefProfile = await _unitOfWork.Chefs.GetByIdAsync(id);
            if (chefProfile == null || chefProfile.Status != "Pending") return false;

            chefProfile.Status = "Rejected";
            chefProfile.RejectionReason = rejectionReason.Trim();

            _unitOfWork.Chefs.Update(chefProfile);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}