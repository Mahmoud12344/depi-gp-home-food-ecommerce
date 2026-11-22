using depiR5gp.ViewModels.AdminDashboard;

namespace depiR5gp.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardVM> GetDashboardAsync(string? range = "month", CancellationToken cancellationToken = default);
}
