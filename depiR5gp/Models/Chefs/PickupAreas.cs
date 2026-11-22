namespace depiR5gp.Models.Chefs;

public static class PickupAreas
{
    public static readonly IReadOnlyList<(string Key, string Name)> All = new List<(string Key, string Name)>
    {
        ("maadi", "Maadi, Cairo"),
        ("zamalek", "Zamalek, Cairo"),
        ("new_cairo", "New Cairo (5th Settlement)"),
        ("heliopolis", "Heliopolis, Cairo"),
        ("nasr_city", "Nasr City, Cairo"),
        ("dokki", "Dokki & Mohandessin, Giza"),
        ("zayed", "Sheikh Zayed, Giza"),
    };

    public static bool IsValid(string? key) => key != null && All.Any(a => a.Key == key);
    public static string NameOf(string? key) => All.FirstOrDefault(a => a.Key == key).Name ?? key ?? "";
}