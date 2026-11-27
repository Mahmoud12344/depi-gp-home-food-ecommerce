namespace depiR5gp.Models.Chefs
{
    public class ChefApplicationStatusVM
    {
        public string Status { get; set; } = string.Empty;
        public string? RejectionNotes { get; set; } // إضافة ? لتقبل null بدون تحذير
    }
}