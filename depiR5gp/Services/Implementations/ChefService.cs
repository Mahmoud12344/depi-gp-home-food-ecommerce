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
            Area = model.Area,
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
}