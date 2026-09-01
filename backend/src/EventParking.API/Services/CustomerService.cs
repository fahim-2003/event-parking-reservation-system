using EventParking.API.DTOs.Customers;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CustomerProfileResponse?> GetOwnProfileAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        return MapProfile(user);
    }

    public async Task<CustomerProfileResponse?> UpdateOwnProfileAsync(
        string userId,
        UpdateCustomerProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Unable to update customer profile: {errors}");
        }

        return MapProfile(user);
    }

    public async Task<IReadOnlyList<AdminCustomerSummaryResponse>>
        SearchCustomersAsync(string? search)
    {
        var customerRole = AppRoles.Customer;

        var usersInRole =
            await _userManager.GetUsersInRoleAsync(customerRole);

        IEnumerable<ApplicationUser> customers = usersInRole;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            customers = customers.Where(user =>
                user.FullName.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase) ||
                (user.Email?.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return customers
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .Select(MapAdminSummary)
            .ToList();
    }

    public async Task<AdminCustomerSummaryResponse?>
        GetCustomerForAdminAsync(string customerId)
    {
        var user = await _userManager.FindByIdAsync(customerId);

        if (user is null)
        {
            return null;
        }

        if (!await _userManager.IsInRoleAsync(
                user,
                AppRoles.Customer))
        {
            return null;
        }

        return MapAdminSummary(user);
    }

    private static CustomerProfileResponse MapProfile(
        ApplicationUser user)
    {
        return new CustomerProfileResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailConfirmed,
            AccountStatus = user.AccountStatus.ToString(),
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }

    private static AdminCustomerSummaryResponse MapAdminSummary(
        ApplicationUser user)
    {
        return new AdminCustomerSummaryResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailConfirmed,
            AccountStatus = user.AccountStatus.ToString(),
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}