using System.ComponentModel.DataAnnotations;

namespace depiR5gp.Models.Chefs
{
    public class ChefApplicationViewModel
    {
        [Required(ErrorMessage = "اسم المطبخ مطلوب.")]
        [Display(Name = "اسم المطبخ")]
        [StringLength(100, ErrorMessage = "اسم المطبخ يجب ألا يتجاوز 100 حرف.")]
        public string KitchenName { get; set; } = string.Empty;

        [Required(ErrorMessage = "العنوان تفصيلياً مطلوب.")]
        [Display(Name = "عنوان المطبخ")]
        [StringLength(200, ErrorMessage = "العنوان يجب ألا يتجاوز 200 حرف.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى رفع صورة أو ملف بطاقة الهوية.")]
        [Display(Name = "إثبات الهوية (PDF/JPG/PNG)")]
        public IFormFile IdDocument { get; set; } = null!;

        [Required(ErrorMessage = "يرجى رفع الشهادة الصحية.")]
        [Display(Name = "الشهادة الصحية (PDF/JPG/PNG)")]
        public IFormFile HealthCertificate { get; set; } = null!;

        [Display(Name = "ترخيص المطبخ / ممارسة المهنة (إن وجد)")]
        public IFormFile? CulinaryLicense { get; set; }
    }
}
