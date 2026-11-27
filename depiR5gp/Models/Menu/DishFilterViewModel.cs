using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using depiR5gp.Models.Chefs;

namespace depiR5gp.Models.Menu;

public class DishFilterViewModel
{
    public string? Search { get; set; }
    [FromQuery(Name = "category")] public string? CategoryParam { get; set; }
    public int? CuisineId { get; set; }
    public string? CategorySlug { get; set; }
    public string? Area { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "featured";

    public static string? SlugLabel(string? slug) => slug?.ToLowerInvariant() switch
    {
        "egyptian" => "Traditional Egyptian",
        "tagines" => "Clay Tagines",
        "bakery" => "Artisan Bakery",
        "alexandrian" => "Alexandrian Seafood",
        "desserts" => "Oriental Desserts",
        _ => null
    };

    public void Normalize()
    {
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

        if (!string.IsNullOrWhiteSpace(CategoryParam))
        {
            string trimmed = CategoryParam.Trim();
            if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedId) && parsedId > 0)
            {
                CuisineId = parsedId;
                CategorySlug = null;
            }
            else if (SlugLabel(trimmed) is not null)
            {
                CategorySlug = trimmed.ToLowerInvariant();
                CuisineId = null;
            }
            else
            {
                CuisineId = null;
                CategorySlug = null;
            }
        }

        if (!PickupAreas.IsValid(Area)) Area = null;          // بيحوّل "all" لـ null كمان
        if (MinPrice < 0) MinPrice = null;
        if (MaxPrice < 0) MaxPrice = null;
        if (MinPrice > MaxPrice) (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
        if (Sort is not ("price_asc" or "price_desc" or "rating")) Sort = "featured";
    }

    public bool HasActiveFilters => Search != null || CuisineId != null || CategorySlug != null
                                    || Area != null || MinPrice != null || MaxPrice != null;

    // بيبني لينك الصفحة من غير فلتر معين (للـ chips): "search" | "category" | "area" | "price"
    public string BuildUrl(string? without = null)
    {
        var parts = new List<string>();
        void Add(string k, string? v) { if (!string.IsNullOrEmpty(v)) parts.Add($"{k}={Uri.EscapeDataString(v)}"); }
        if (without != "search") Add("search", Search);
        if (without != "category") Add("category", CuisineId?.ToString(CultureInfo.InvariantCulture) ?? CategorySlug);
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