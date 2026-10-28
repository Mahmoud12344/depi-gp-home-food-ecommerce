using depiR5gp.Models.Chefs;
using depiR5gp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using depiR5gp.Services.Interfaces;

namespace depiR5gp.Controllers;

//[Authorize] comment for test
public class ChefsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ApplicationDbContext context,IFileService fileService) : Controller
{
    public IActionResult Index() => View();

    public IActionResult Details(int id) => View();

    [HttpGet]
    public async Task<IActionResult> Onboarding()
    {
        int userId = GetCurrentUserId();

        // AC2: منع فتح الصفحة إذا كان لدى المستخدم طلب قيد الانتظار أو مقبول
        if (await unitOfWork.Chefs.HasPendingOrApprovedApplicationAsync(userId))
        {
            TempData["ErrorMessage"] = "You already have a pending application or your chef account is already activated."; 
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboarding(ChefApplicationViewModel model) {
        const string  ChefFolder = "chef_documents";
        var documents = new (IFormFile? File, string DocType)[]
        {
            (model.IdDocument,        "Identity"),
            (model.HealthCertificate, "HealthCertificate"),
            (model.CulinaryLicense,   "CulinaryLicense"),
        };

        int userId = GetCurrentUserId();

        // فحص تكرار الطلب عند الإرسال
        if (await unitOfWork.Chefs.HasPendingOrApprovedApplicationAsync(userId))
        {
            ModelState.AddModelError(string.Empty, "لديك بالفعل طلب قيد المراجعة أو تم تفعيل حسابك مسبقاً.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // إنشاء كائن ChefProfile
        var chefProfile = new ChefProfile
        {
            UserId = userId,
            KitchenName = model.KitchenName,
            Address = model.Address,
            Status = "Pending",
            HasAcceptedTerms = true
        };

        // رفع وحفظ الملفات في مجلد wwwroot/uploads/chef_documents
        foreach (var (file, docType) in documents)
        {
            if (file is null) continue;

            var url = await fileService.SaveAsync(file, ChefFolder, docType);
            chefProfile.Documents.Add(new ChefDocument { DocType = docType, FileUrl = url });
        }
        // إضافة البيانات وحفظها عبر UnitOfWork
        await unitOfWork.Chefs.AddAsync(chefProfile);
        await unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تقديم طلبك بنجاح وسيتم مراجعته من قبل الأدمن.";
        return RedirectToAction("Index", "Home");
    }

    private int GetCurrentUserId()
    {
        // 1. محاولة جلب ID المستخدم إذا كان مسجلاً الدخول
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out int userId) && userId > 0)
        {
            return userId;
        }

        // 2. جلب ID أول مستخدم مسجل فعلياً في قاعدة البيانات
        var existingUser = context.Users.FirstOrDefault();
        if (existingUser != null)
        {
            return existingUser.Id;
        }

        // 3. في حال كانت قاعدة البيانات فارغة، إنشاء مستخدم تجريبي تلقائياً
        var dummyUser = new depiR5gp.Models.Users.AppUser
        {
            UserName = "testchef@example.com",
            Email = "testchef@example.com"
        };
        context.Users.Add(dummyUser);
        context.SaveChanges();

        return dummyUser.Id;
    }

    // private async Task<string> SaveFileAsync(IFormFile file, string docType)
    // {
    //     string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "chef_documents");
    //     if (!Directory.Exists(uploadsFolder))
    //     {
    //         Directory.CreateDirectory(uploadsFolder);
    //     }
    //  
    //     string uniqueFileName = $"{Guid.NewGuid()}_{docType}{Path.GetExtension(file.FileName)}";
    //     string filePath = Path.Combine(uploadsFolder, uniqueFileName);
    //
    //     using (var fileStream = new FileStream(filePath, FileMode.Create))
    //     {
    //         await file.CopyToAsync(fileStream);
    //     }
    //
    //     return $"/uploads/chef_documents/{uniqueFileName}";
    // }
}