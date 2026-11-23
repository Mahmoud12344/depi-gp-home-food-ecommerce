using depiR5gp.Data;
using depiR5gp.Models.Chefs;
using Microsoft.EntityFrameworkCore;

namespace depiR5gp.ViewModels;

public class ChefPortalHeaderVM
{
    public int ChefProfileId { get; set; }
    public string ChefName { get; set; } = string.Empty;
    public string KitchenName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public bool IsAcceptingOrders { get; set; }
    public string Status { get; set; } = "Pending";
    public int PendingOrdersCount { get; set; }
    public int PreparingOrdersCount { get; set; }
    public int ReadyOrdersCount { get; set; }
    public int ActiveOrdersCount => PendingOrdersCount + PreparingOrdersCount + ReadyOrdersCount;
    public int DishesCount { get; set; }
    public int ReviewsCount { get; set; }
    public decimal NetWalletEarnings { get; set; }

    public static async Task<ChefPortalHeaderVM> LoadAsync(ApplicationDbContext context, ChefProfile chef)
    {
        var chefUser = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == chef.UserId)
            .Select(u => new { u.FirstName, u.LastName, u.ProfileImagePath })
            .FirstOrDefaultAsync();

        var chefFullName = chefUser is not null
            ? $"{chefUser.FirstName} {chefUser.LastName}".Trim()
            : string.Empty;
        var resolvedPhoto = !string.IsNullOrWhiteSpace(chef.PhotoUrl)
            ? chef.PhotoUrl
            : chefUser?.ProfileImagePath;

        var orderCounts = await context.Orders
            .AsNoTracking()
            .Where(o => o.ChefProfileId == chef.Id)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var dishesCount = await context.Dishes
            .AsNoTracking()
            .CountAsync(d => d.ChefProfileId == chef.Id && !d.IsArchived);

        var reviewsCount = await context.Reviews
            .AsNoTracking()
            .CountAsync(r => r.ChefProfileId == chef.Id && r.ModerationStatus == "Approved");

        var netEarnings = await context.Orders
            .AsNoTracking()
            .Where(o => o.ChefProfileId == chef.Id && o.Status == "Completed")
            .SumAsync(o => (decimal?)(o.Total - o.CommissionAmount)) ?? 0m;

        return new ChefPortalHeaderVM
        {
            ChefProfileId = chef.Id,
            ChefName = !string.IsNullOrWhiteSpace(chefFullName) ? chefFullName : chef.KitchenName,
            KitchenName = chef.KitchenName,
            PhotoUrl = resolvedPhoto,
            AreaName = !string.IsNullOrWhiteSpace(chef.Area)
                ? PickupAreas.NameOf(chef.Area)
                : (chef.Address ?? string.Empty),
            IsAcceptingOrders = chef.IsAcceptingOrders,
            Status = chef.Status,
            PendingOrdersCount = orderCounts.Where(x => x.Status == "Pending").Sum(x => x.Count),
            PreparingOrdersCount = orderCounts.Where(x => x.Status == "Preparing" || x.Status == "Accepted").Sum(x => x.Count),
            ReadyOrdersCount = orderCounts.Where(x => x.Status == "Ready").Sum(x => x.Count),
            DishesCount = dishesCount,
            ReviewsCount = reviewsCount,
            NetWalletEarnings = netEarnings
        };
    }
}

public class ChefWorkingDayVM
{
    public byte DayOfWeek { get; set; } // 0=Sunday .. 6=Saturday
    public string DayName { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = "12:00";
    public string CloseTime { get; set; } = "22:00";
}

public class ChefWorkingHoursVM
{
    public List<ChefWorkingDayVM> Days { get; set; } = [];
}

public class ChefDocumentsVM
{
    public string KitchenName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public List<ChefDocument> Documents { get; set; } = [];
}
