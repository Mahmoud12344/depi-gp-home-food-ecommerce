using depiR5gp.Models.Chefs;
using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Implementations;
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
        // 1. فحص تكرار الطلب
        if (await HasPendingOrApprovedApplicationAsync(userId))
        {
            return (false, "You already have a pending application or your chef account is already activated.");
        }

        // 2. تجهيز كائن البيانات
        var chefProfile = new ChefProfile
        {
            UserId = userId,
            KitchenName = model.KitchenName,
            Address = model.Address,
            Status = "Pending",
            HasAcceptedTerms = true
        };

        // 3. معالجة ورفع الملفات
        const string ChefFolder = "chef_documents";
        var documents = new (Microsoft.AspNetCore.Http.IFormFile? File, string DocType)[]
        {
            (model.IdDocument,        "Identity"),
            (model.HealthCertificate, "HealthCertificate"),
            (model.CulinaryLicense,   "CulinaryLicense"),
        };

        foreach (var (file, docType) in documents)
        {
            if (file is null) continue;

            var url = await fileService.SaveAsync(file, ChefFolder, docType);
            chefProfile.Documents.Add(new ChefDocument { DocType = docType, FileUrl = url });
        }

        // 4. الحفظ في قاعدة البيانات
        await unitOfWork.Chefs.AddAsync(chefProfile);
        await unitOfWork.SaveChangesAsync();

        return (true, "Your application has been submitted successfully and will be reviewed by the admin.");
    }

    public async Task<ChefProfile?> GetChefApplicationStatusAsync(int userId)
    {
        return await unitOfWork.Chefs.GetByUserIdAsync(userId);
    }

    // ==========================================
    //  التعديلات الجديدة الخاصة بـ Edit Profile
    // ==========================================

    public async Task<EditChefProfileViewModel?> GetChefProfileForEditAsync(int userId)
    {
        var chef = await unitOfWork.Chefs.GetByUserIdAsync(userId);
        if (chef == null) return null;

        return new EditChefProfileViewModel
        {
            Name = chef.KitchenName,
            Bio = chef.Bio,
            Address = chef.Address,
            ExistingPhotoUrl = chef.PhotoUrl,
            IsAcceptingOrders = chef.IsAcceptingOrders
        };
    }

    public async Task<bool> UpdateChefProfileAsync(int userId, EditChefProfileViewModel model)
    {
        var chef = await unitOfWork.Chefs.GetByUserIdAsync(userId);
        if (chef == null) return false;

        // تحديث البيانات الأساسية
        chef.Bio = model.Bio;
        chef.Address = model.Address;
        chef.IsAcceptingOrders = model.IsAcceptingOrders;

        // رفع الصورة الشخصية في حال تم إرفاق ملف جديد باستخدام IFileService
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            var photoUrl = await fileService.SaveAsync(model.PhotoFile, "chef_profiles", "profile");
            chef.PhotoUrl = photoUrl;
        }

        unitOfWork.Chefs.Update(chef);
        await unitOfWork.SaveChangesAsync();

        return true;
    }
}