using EventParking.API.DTOs.Customers;

namespace EventParking.API.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerProfileResponse?> GetOwnProfileAsync(
        string userId);

    Task<CustomerProfileResponse?> UpdateOwnProfileAsync(
        string userId,
        UpdateCustomerProfileRequest request);

    Task<IReadOnlyList<AdminCustomerSummaryResponse>>
        SearchCustomersAsync(string? search);

    Task<AdminCustomerSummaryResponse?>
        GetCustomerForAdminAsync(string customerId);
}