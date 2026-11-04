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
}