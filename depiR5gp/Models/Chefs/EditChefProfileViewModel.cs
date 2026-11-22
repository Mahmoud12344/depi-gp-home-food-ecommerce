using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace depiR5gp.Models.Chefs
{
    public class EditChefProfileViewModel
    {
        [Display(Name = "الاسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "نبذة عن الشيف")]
        [StringLength(500, ErrorMessage = "النبذة يجب ألا تتجاوز 500 حرف.")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب.")]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "ساعات العمل مطلوبة.")]
        [Display(Name = "ساعات العمل")]
        public string? WorkingHours { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public IFormFile? PhotoFile { get; set; } // لاستقبال الملف المرفوع

        public string? ExistingPhotoUrl { get; set; } // لعرض الصورة الحالية

        public bool IsAcceptingOrders { get; set; }
    }
}