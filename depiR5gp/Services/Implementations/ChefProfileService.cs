using System.Globalization;
using depiR5gp.Models.Chefs;
using depiR5gp.Models.Users;
using depiR5gp.Repositories.Interfaces;

namespace depiR5gp.Services;

public class ChefProfileService : IChefProfileService
{
    private static readonly string[] DayNames =
    [
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday"
    ];

    private readonly IUnitOfWork _unitOfWork;

    public ChefProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChefProfileViewModel?> GetChefProfileAsync(int id)
    {
        var profile = await _unitOfWork.Chefs.GetByIdWithDetailsAsync(id);

        if (profile == null || profile.Status != "Approved" || profile.User?.Status != UserStatus.Active)
        {
            return null;
        }

        var approvedReviews = profile.Reviews
            .Where(r => r.ModerationStatus == "Approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        double averageRating = approvedReviews.Count > 0
            ? Math.Round(approvedReviews.Average(r => (double)r.Rating), 1)
            : 0;

        var formattedHours = profile.WorkingHours
            .OrderBy(w => w.DayOfWeek)
            .Select(w =>
            {
                string dayName = w.DayOfWeek >= 0 && w.DayOfWeek < DayNames.Length
                    ? DayNames[w.DayOfWeek]
                    : $"Day {w.DayOfWeek}";
                string openFormatted = DateTime.Today.Add(w.OpenTime.ToTimeSpan()).ToString("h:mm tt", CultureInfo.InvariantCulture);
                string closeFormatted = DateTime.Today.Add(w.CloseTime.ToTimeSpan()).ToString("h:mm tt", CultureInfo.InvariantCulture);
                return $"{dayName}: {openFormatted} – {closeFormatted}";
            })
            .ToList();

        return new ChefProfileViewModel
        {
            Id = profile.Id,
            Name = !string.IsNullOrEmpty(profile.KitchenName)
                ? profile.KitchenName
                : $"{profile.User?.FirstName} {profile.User?.LastName}".Trim(),
            Bio = profile.Bio,
            PhotoUrl = profile.PhotoUrl,
            Address = profile.Address,
            WorkingHours = formattedHours,
            IsAcceptingOrders = profile.IsAcceptingOrders,
            AverageRating = averageRating,
            ReviewCount = approvedReviews.Count,
            Reviews = approvedReviews.Select(r => new ChefPublicReviewItemVM
            {
                CustomerName = r.User != null
                    ? $"{r.User.FirstName} {r.User.LastName}".Trim()
                    : "Verified Customer",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }
}