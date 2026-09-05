using EventParking.API.DTOs.Dashboard;

namespace EventParking.API.Interfaces.Services.Dashboard;

public interface IDashboardService
{
    Task<AdminDashboardResponse> GetAdminDashboardAsync(
        CancellationToken cancellationToken);

    Task<CustomerDashboardResponse> GetCustomerDashboardAsync(
        string customerId,
        CancellationToken cancellationToken);
}
