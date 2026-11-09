using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Models.Menu;

public class DishFilterViewModel
{
    public string? Search { get; set; }
    [FromQuery(Name = "category")] public int? CuisineId { get; set; }   // الـ drawer بيبعت اسمه category
    public string? Area { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "featured";

    public void Normalize()
    {
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
        if (!PickupAreas.IsValid(Area)) Area = null;          // بيحوّل "all" لـ null كمان
        if (MinPrice < 0) MinPrice = null;
        if (MaxPrice < 0) MaxPrice = null;
        if (MinPrice > MaxPrice) (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
        if (Sort is not ("price_asc" or "price_desc" or "rating")) Sort = "featured";
    }

    public bool HasActiveFilters => Search != null || CuisineId != null || Area != null
                                    || MinPrice != null || MaxPrice != null;

    // بيبني لينك الصفحة من غير فلتر معين (للـ chips): "search" | "category" | "area" | "price"
    public string BuildUrl(string? without = null)
    {
        var parts = new List<string>();
        void Add(string k, string? v) { if (!string.IsNullOrEmpty(v)) parts.Add($"{k}={Uri.EscapeDataString(v)}"); }
        if (without != "search") Add("search", Search);
        if (without != "category") Add("category", CuisineId?.ToString());
        if (without != "area") Add("area", Area);
        if (without != "price")
        {
            Add("minPrice", MinPrice?.ToString(CultureInfo.InvariantCulture));
            Add("maxPrice", MaxPrice?.ToString(CultureInfo.InvariantCulture));
        }
        if (Sort != "featured") Add("sort", Sort);
        return parts.Count == 0 ? "/Dishes" : "/Dishes?" + string.Join("&", parts);
    }
}