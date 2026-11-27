using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Services.Implementations;

public class ChefService(IUnitOfWork unitOfWork, IFileService fileService) : IChefService
{
    public async Task<bool> HasPendingOrApprovedApplicationAsync(int userId)
    {
        return await unitOfWork.Chefs.HasPendingOrApprovedApplicationAsync(userId);
    }

    public async Task<(bool IsSuccess, string Message)> SubmitApplicationAsync(int userId, ChefApplicationViewModel model)
    {
        var existingProfile = await unitOfWork.Chefs.GetByUserIdAsync(userId);
        if (existingProfile is not null && (existingProfile.Status == "Pending" || existingProfile.Status == "Approved"))
        {
            return (false, "You already have a pending application or your chef account is already activated.");
        }

        decimal commissionRate = await GetCommissionRate();

        const string ChefFolder = "chef_documents";
        var documents = new (Microsoft.AspNetCore.Http.IFormFile? File, string DocType)[]
        {
            (model.IdDocument,        "Identity"),
            (model.HealthCertificate, "HealthCertificate"),
            (model.CulinaryLicense,   "CulinaryLicense"),
        };

        try
        {
            if (existingProfile is not null)
            {
                // Re-submission for a previously Rejected application: update in-place to avoid IX_ChefProfiles_user_id collision
                existingProfile.KitchenName = model.KitchenName.Trim();
                existingProfile.Address = model.Address?.Trim();
                existingProfile.Area = model.Area;
                existingProfile.Status = "Pending";
                existingProfile.RejectionReason = null;
                existingProfile.HasAcceptedTerms = true;
                existingProfile.CommissionRate = commissionRate;

                foreach (var (file, docType) in documents)
                {
                    if (file is null || file.Length <= 0) continue;

                    var url = await fileService.SaveAsync(file, ChefFolder, docType);
                    var prevDoc = existingProfile.Documents.FirstOrDefault(d =>
                        string.Equals(d.DocType, docType, StringComparison.OrdinalIgnoreCase));

                    if (prevDoc is not null)
                    {
                        await fileService.DeleteAsync(prevDoc.FileUrl);
                        prevDoc.FileUrl = url;
                        prevDoc.UploadedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        existingProfile.Documents.Add(new ChefDocument
                        {
                            ChefProfileId = existingProfile.Id,
                            DocType = docType,
                            FileUrl = url,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                unitOfWork.Chefs.Update(existingProfile);
                await unitOfWork.SaveChangesAsync();

                return (true, "Your updated application has been resubmitted and will be reviewed by the admin.");
            }

            var chefProfile = new ChefProfile
            {
                UserId = userId,
                KitchenName = model.KitchenName.Trim(),
                Address = model.Address?.Trim(),
                Area = model.Area,
                Status = "Pending",
                HasAcceptedTerms = true,
                CommissionRate = commissionRate
            };

            foreach (var (file, docType) in documents)
            {
                if (file is null || file.Length <= 0) continue;

                var url = await fileService.SaveAsync(file, ChefFolder, docType);
                chefProfile.Documents.Add(new ChefDocument { DocType = docType, FileUrl = url });
            }

            await unitOfWork.Chefs.AddAsync(chefProfile);
            await unitOfWork.SaveChangesAsync();

            return (true, "Your application has been submitted successfully and will be reviewed by the admin.");
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<ChefProfile?> GetChefApplicationStatusAsync(int userId)
    {
        return await unitOfWork.Chefs.GetByUserIdAsync(userId);
    }

    public async Task<decimal> GetCommissionRate()
    {
        var settings = await unitOfWork.PlatformSettingRepository.GetAllAsync();
        return settings.FirstOrDefault()?.CommissionRate ?? 0.15m;
    }

    public async Task<ChefCatalogViewModel> GetCatalogAsync(ChefFilterViewModel filter)
    {
        filter.Normalize();
        var areas = await unitOfWork.Chefs.GetAreaOptionsAsync();
        foreach (var a in areas) a.Name = PickupAreas.NameOf(a.Value);
        return new ChefCatalogViewModel
        {
            Filter = filter,
            Chefs = await unitOfWork.Chefs.SearchAsync(filter),
            Areas = areas
        };
    }

    public async Task<EditChefProfileViewModel?> GetChefProfileForEditAsync(int userId)
    {
        var chef = await unitOfWork.Chefs.GetByUserIdAsync(userId);
        if (chef == null) return null;

        return new EditChefProfileViewModel
        {
            Name = chef.KitchenName,
            Bio = chef.Bio,
            Address = chef.Address,
            Area = chef.Area,
            ExistingPhotoUrl = chef.PhotoUrl,
            IsAcceptingOrders = chef.IsAcceptingOrders
        };
    }

    public async Task<bool> UpdateChefProfileAsync(int userId, EditChefProfileViewModel model)
    {
        var chef = await unitOfWork.Chefs.GetByUserIdAsync(userId);
        if (chef == null) return false;

        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            chef.KitchenName = model.Name.Trim();
        }
        chef.Bio = model.Bio;
        chef.Address = model.Address;
        if (PickupAreas.IsValid(model.Area))
        {
            chef.Area = model.Area;
        }
        chef.IsAcceptingOrders = model.IsAcceptingOrders;

        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            var oldPhotoUrl = chef.PhotoUrl;
            var photoUrl = await fileService.SaveAsync(model.PhotoFile, "chef_profiles", "profile");
            if (!string.IsNullOrWhiteSpace(oldPhotoUrl))
            {
                await fileService.DeleteAsync(oldPhotoUrl);
            }
            chef.PhotoUrl = photoUrl;
        }

        unitOfWork.Chefs.Update(chef);
        await unitOfWork.SaveChangesAsync();

        return true;
    }
}
