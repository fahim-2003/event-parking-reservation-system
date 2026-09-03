using EventParking.API.Enums;
using EventParking.API.Identity;
using EventParking.API.Services;
using EventParking.API.DTOs.Customers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace EventParking.Tests.Customers;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task GetOwnProfileAsync_ReturnsMappedCustomerProfile()
    {
        var user = new ApplicationUser
        {
            Id = "customer-1",
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            EmailConfirmed = true,
            AccountStatus = AccountStatus.Active,
            CreatedAtUtc = new DateTime(
                2026,
                9,
                1,
                7,
                0,
                0,
                DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(
                2026,
                9,
                1,
                8,
                0,
                0,
                DateTimeKind.Utc)
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByIdAsync("customer-1"))
            .ReturnsAsync(user);

        var service = new CustomerService(
            userManager.Object);

        var result =
            await service.GetOwnProfileAsync("customer-1");

        Assert.NotNull(result);
        Assert.Equal("customer-1", result.UserId);
        Assert.Equal("Test Customer", result.FullName);
        Assert.Equal(
            "customer1@eventparking.local",
            result.Email);
        Assert.Equal(
            "0771234567",
            result.PhoneNumber);
        Assert.True(result.EmailVerified);
        Assert.Equal(
            AccountStatus.Active.ToString(),
            result.AccountStatus);
        Assert.Equal(
            user.CreatedAtUtc,
            result.CreatedAtUtc);
        Assert.Equal(
            user.UpdatedAtUtc,
            result.UpdatedAtUtc);
    }

    [Fact]
    public async Task GetOwnProfileAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByIdAsync("missing-user"))
            .ReturnsAsync(
                (ApplicationUser?)null);

        var service = new CustomerService(
            userManager.Object);

        var result =
            await service.GetOwnProfileAsync("missing-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateOwnProfileAsync_TrimsAndPersistsEditableFields()
    {
        var user = new ApplicationUser
        {
            Id = "customer-1",
            FullName = "Old Name",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0700000000",
            EmailConfirmed = true,
            AccountStatus = AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByIdAsync("customer-1"))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.UpdateAsync(user))
            .ReturnsAsync(
                IdentityResult.Success);

        var service = new CustomerService(
            userManager.Object);

        var request =
            new UpdateCustomerProfileRequest
            {
                FullName = "  Updated Customer  ",
                PhoneNumber = "  0777654321  "
            };

        var result =
            await service.UpdateOwnProfileAsync(
                "customer-1",
                request);

        Assert.NotNull(result);

        Assert.Equal(
            "Updated Customer",
            user.FullName);

        Assert.Equal(
            "0777654321",
            user.PhoneNumber);

        Assert.Equal(
            "Updated Customer",
            result.FullName);

        Assert.Equal(
            "0777654321",
            result.PhoneNumber);

        userManager.Verify(
            manager =>
                manager.UpdateAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task SearchCustomersAsync_FiltersAndSortsCustomers()
    {
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = "customer-2",
                FullName = "Zara Customer",
                Email = "zara@eventparking.local",
                AccountStatus = AccountStatus.Active
            },
            new()
            {
                Id = "customer-1",
                FullName = "Alice Customer",
                Email = "alice@eventparking.local",
                AccountStatus = AccountStatus.Active
            },
            new()
            {
                Id = "customer-3",
                FullName = "Different Person",
                Email = "different@example.local",
                AccountStatus = AccountStatus.Active
            }
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.GetUsersInRoleAsync(
                    AppRoles.Customer))
            .ReturnsAsync(users);

        var service = new CustomerService(
            userManager.Object);

        var result =
            await service.SearchCustomersAsync(
                "customer");

        Assert.Equal(2, result.Count);

        Assert.Equal(
            "Alice Customer",
            result[0].FullName);

        Assert.Equal(
            "Zara Customer",
            result[1].FullName);
    }

    [Fact]
    public async Task GetCustomerForAdminAsync_ReturnsCustomer_WhenRoleMatches()
    {
        var user = new ApplicationUser
        {
            Id = "customer-1",
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            EmailConfirmed = true,
            AccountStatus = AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByIdAsync("customer-1"))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.IsInRoleAsync(
                    user,
                    AppRoles.Customer))
            .ReturnsAsync(true);

        var service = new CustomerService(
            userManager.Object);

        var result =
            await service.GetCustomerForAdminAsync(
                "customer-1");

        Assert.NotNull(result);
        Assert.Equal(
            "customer-1",
            result.UserId);
        Assert.Equal(
            "Test Customer",
            result.FullName);
    }

    [Fact]
    public async Task GetCustomerForAdminAsync_ReturnsNull_WhenUserIsNotCustomer()
    {
        var user = new ApplicationUser
        {
            Id = "admin-1",
            FullName = "System Administrator",
            Email = "admin@eventparking.local",
            AccountStatus = AccountStatus.Active
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByIdAsync("admin-1"))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.IsInRoleAsync(
                    user,
                    AppRoles.Customer))
            .ReturnsAsync(false);

        var service = new CustomerService(
            userManager.Object);

        var result =
            await service.GetCustomerForAdminAsync(
                "admin-1");

        Assert.Null(result);
    }

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManagerMock()
    {
        var store =
            new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            null!,
            new IdentityErrorDescriber(),
            null!,
            null!);
    }
}
