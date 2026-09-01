using System.Reflection;
using System.Security.Claims;
using EventParking.API.Controllers;
using EventParking.API.DTOs.Customers;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventParking.Tests.Controllers;

public sealed class CustomersControllerTests
{
    [Fact]
    public async Task GetOwnProfile_ReturnsUnauthorized_WhenUserIdClaimMissing()
    {
        var customerService =
            new Mock<ICustomerService>();

        var controller =
            CreateController(
                customerService,
                Array.Empty<Claim>());

        var result =
            await controller.GetOwnProfile();

        Assert.IsType<UnauthorizedResult>(result);

        customerService.Verify(
            service =>
                service.GetOwnProfileAsync(
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOwnProfile_ReturnsCustomerProfile_ForAuthenticatedCustomer()
    {
        var customerService =
            new Mock<ICustomerService>();

        var profile =
            CreateProfile();

        customerService
            .Setup(service =>
                service.GetOwnProfileAsync(
                    "customer-1"))
            .ReturnsAsync(profile);

        var controller =
            CreateController(
                customerService,
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "customer-1")
                });

        var result =
            await controller.GetOwnProfile();

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<CustomerProfileResponse>(
                okResult.Value);

        Assert.Equal(
            "customer-1",
            response.UserId);

        Assert.Equal(
            "customer1@eventparking.local",
            response.Email);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsUpdatedProfile_ForAuthenticatedCustomer()
    {
        var customerService =
            new Mock<ICustomerService>();

        var request =
            new UpdateCustomerProfileRequest
            {
                FullName = "Updated Customer",
                PhoneNumber = "0771112233"
            };

        var profile =
            CreateProfile(
                fullName: "Updated Customer",
                phoneNumber: "0771112233");

        customerService
            .Setup(service =>
                service.UpdateOwnProfileAsync(
                    "customer-1",
                    request))
            .ReturnsAsync(profile);

        var controller =
            CreateController(
                customerService,
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "customer-1")
                });

        var result =
            await controller.UpdateOwnProfile(
                request);

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<CustomerProfileResponse>(
                okResult.Value);

        Assert.Equal(
            "Updated Customer",
            response.FullName);

        Assert.Equal(
            "0771112233",
            response.PhoneNumber);
    }

    [Fact]
    public async Task SearchCustomers_ReturnsAdminCustomerList()
    {
        var customerService =
            new Mock<ICustomerService>();

        IReadOnlyList<AdminCustomerSummaryResponse>
            customers =
            new[]
            {
                CreateAdminSummary()
            };

        customerService
            .Setup(service =>
                service.SearchCustomersAsync(
                    "customer"))
            .ReturnsAsync(customers);

        var controller =
            CreateController(
                customerService,
                Array.Empty<Claim>());

        var result =
            await controller.SearchCustomers(
                "customer");

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsAssignableFrom<
                IReadOnlyList<
                    AdminCustomerSummaryResponse>>(
                okResult.Value);

        Assert.Single(response);

        Assert.Equal(
            "customer-1",
            response[0].UserId);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        var customerService =
            new Mock<ICustomerService>();

        customerService
            .Setup(service =>
                service.GetCustomerForAdminAsync(
                    "missing-customer"))
            .ReturnsAsync(
                (AdminCustomerSummaryResponse?)null);

        var controller =
            CreateController(
                customerService,
                Array.Empty<Claim>());

        var result =
            await controller.GetCustomer(
                "missing-customer");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetOwnProfile_RequiresCustomerRole()
    {
        var authorize =
            GetAuthorizeAttribute(
                nameof(
                    CustomersController
                        .GetOwnProfile));

        Assert.Equal(
            AppRoles.Customer,
            authorize.Roles);
    }

    [Fact]
    public void UpdateOwnProfile_RequiresCustomerRole()
    {
        var authorize =
            GetAuthorizeAttribute(
                nameof(
                    CustomersController
                        .UpdateOwnProfile));

        Assert.Equal(
            AppRoles.Customer,
            authorize.Roles);
    }

    [Fact]
    public void SearchCustomers_RequiresAdministratorRole()
    {
        var authorize =
            GetAuthorizeAttribute(
                nameof(
                    CustomersController
                        .SearchCustomers));

        Assert.Equal(
            AppRoles.Administrator,
            authorize.Roles);
    }

    [Fact]
    public void GetCustomer_RequiresAdministratorRole()
    {
        var authorize =
            GetAuthorizeAttribute(
                nameof(
                    CustomersController
                        .GetCustomer));

        Assert.Equal(
            AppRoles.Administrator,
            authorize.Roles);
    }

    private static CustomersController
        CreateController(
            Mock<ICustomerService> customerService,
            IEnumerable<Claim> claims)
    {
        var identity =
            new ClaimsIdentity(
                claims,
                "TestAuthentication");

        var user =
            new ClaimsPrincipal(identity);

        return new CustomersController(
            customerService.Object)
        {
            ControllerContext =
                new ControllerContext
                {
                    HttpContext =
                        new DefaultHttpContext
                        {
                            User = user
                        }
                }
        };
    }

    private static AuthorizeAttribute
        GetAuthorizeAttribute(
            string methodName)
    {
        var method =
            typeof(CustomersController)
                .GetMethod(methodName);

        Assert.NotNull(method);

        var attribute =
            method.GetCustomAttribute<
                AuthorizeAttribute>();

        Assert.NotNull(attribute);

        return attribute;
    }

    private static CustomerProfileResponse
        CreateProfile(
            string fullName = "Test Customer",
            string phoneNumber = "0771234567")
    {
        return new CustomerProfileResponse
        {
            UserId = "customer-1",
            FullName = fullName,
            Email =
                "customer1@eventparking.local",
            PhoneNumber = phoneNumber,
            EmailVerified = true,
            AccountStatus = "Active",
            CreatedAtUtc =
                new DateTime(
                    2026,
                    9,
                    1,
                    7,
                    0,
                    0,
                    DateTimeKind.Utc),
            UpdatedAtUtc =
                new DateTime(
                    2026,
                    9,
                    1,
                    8,
                    0,
                    0,
                    DateTimeKind.Utc)
        };
    }

    private static AdminCustomerSummaryResponse
        CreateAdminSummary()
    {
        return new AdminCustomerSummaryResponse
        {
            UserId = "customer-1",
            FullName = "Test Customer",
            Email =
                "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            EmailVerified = true,
            AccountStatus = "Active",
            CreatedAtUtc =
                new DateTime(
                    2026,
                    9,
                    1,
                    7,
                    0,
                    0,
                    DateTimeKind.Utc)
        };
    }
}
